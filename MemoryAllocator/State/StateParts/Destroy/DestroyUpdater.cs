using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	internal readonly unsafe ref struct DestroyUpdater
	{
		private readonly World _world;
		private readonly SafePtr<EntityStatePart> _entitiesStatePart;
		private readonly ArchetypeContext<DestroyRequest> _destroyRequestArchetype;
		private readonly ArchetypeContext<KillRequest> _killRequestArchetype;
		private readonly ArchetypeContext<DelayKillRequest> _delayKillRequestArchetype;
		private readonly ArchetypeContext<KillElement> _killElementsArchetype;

		public DestroyUpdater(World world)
		{
			_world = world;
			_entitiesStatePart = _world.GetServicePtr<EntityStatePart>();
			_destroyRequestArchetype = new (_world);
			_killRequestArchetype = new (_world);
			_delayKillRequestArchetype = new (_world);
			_killElementsArchetype = new (_world);
		}

		public void Update(float deltaTime)
		{
			DestroyEntities();
			KillEntities(deltaTime);
		}

		/// <summary>
		/// Уничтожает все entity, которые были помечены для уничтожения.
		/// </summary>
		private void DestroyEntities()
		{
			var count = _destroyRequestArchetype.Count;
			if (count < 1)
				return;

			var destroyRequests = _destroyRequestArchetype.GetRawElements();
			var elementsToDestroy = stackalloc Entity[count];

			for (var i = 0; i < count; i++)
			{
				elementsToDestroy[i] = destroyRequests[i].entity;
			}

			_entitiesStatePart.ptr->DestroyEntities(_world, elementsToDestroy, count);
		}

		/// <summary>
		/// Добавляет все entity, которые были помечены для убийства в список для дальшейшего уничтожения.
		/// Все эти entity живут ещё один тик.
		/// </summary>
		private void KillEntities(float deltaTime)
		{
			var delayKillRequests = _delayKillRequestArchetype.GetRawElements();

			for (var i = 0; i < _delayKillRequestArchetype.Count; i++)
			{
				ref var request = ref delayKillRequests[i];
				if (request.value.delay <= 0)
					_delayKillRequestArchetype.GetElement(request.entity);
				else
					request.value.delay -= deltaTime;
			}

			var killRequestsCount = _killRequestArchetype.Count;
			if (killRequestsCount > 0)
			{
				var killRequestsTempRaw = stackalloc ArchetypeElement<KillRequest>[killRequestsCount];
				var killRequestsTemp = new SafePtr<ArchetypeElement<KillRequest>>(killRequestsTempRaw, killRequestsCount);

				var killRequests = _killRequestArchetype.GetRawElements();
				MemoryExt.MemCopy<ArchetypeElement<KillRequest>>(killRequests, killRequestsTemp, killRequestsCount);

				_killRequestArchetype.Clear();

				for (var i = 0; i < killRequestsCount; i++)
				{
					var entity = killRequestsTemp[i].entity;

					ExecuteKillCallback(entity);
					KillChild(entity);
					KillParent(entity);

					_delayKillRequestArchetype.RemoveSwapBackElement(entity);
					_destroyRequestArchetype.GetElement(entity);
				}
			}
		}

		/// <summary>
		/// Вызывает ивент, уведомляющий подписчиков об убийстве entity.
		/// Затем чистит список подписчиков.
		/// </summary>
		private void ExecuteKillCallback(in Entity entity)
		{
			ref var destroyElement = ref _killElementsArchetype.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (destroyElement.killCallbacks.IsCreated)
			{
				for (var i = 0; i < destroyElement.killCallbacks.Count; i++)
				{
					ref var killCallback = ref destroyElement.killCallbacks[_world, i];

					if (killCallback.callback.IsCreated)
					{
						killCallback.callback.EntityKilled(_world, _world, killCallback.target);
						killCallback.callback.Dispose(_world);
					}

					ref var targetElement = ref _killElementsArchetype.TryGetElement(killCallback.target, out var isTargetExist);
					if (!isTargetExist || !_entitiesStatePart.ptr->IsEntityExist(_world, killCallback.target))
						continue;
					if (targetElement.killCallbackHolders.IsCreated)
						continue;

					for (var j = 0; j < targetElement.killCallbackHolders.Count; j++)
					{
						if (targetElement.killCallbackHolders[_world, i].id == entity.id)
							targetElement.killCallbackHolders.RemoveAtSwapBack(_world, i);
					}
				}
				destroyElement.killCallbacks.Clear();
			}

			if (destroyElement.killCallbackHolders.IsCreated)
			{
				for (var i = 0; i < destroyElement.killCallbackHolders.Count; i++)
				{
					var holder = destroyElement.killCallbackHolders[_world, i];
					if (!_killElementsArchetype.HasElement(holder) || !_entitiesStatePart.ptr->IsEntityExist(_world, holder))
						continue;
					ref var holderElement = ref _killElementsArchetype.GetElement(holder);
					for (var j = 0; j < holderElement.killCallbacks.Count; j++)
					{
						if (holderElement.killCallbacks[_world, j].target.id == entity.id)
						{
							holderElement.killCallbacks[_world, j].callback.Dispose(_world);
							holderElement.killCallbacks.RemoveAtSwapBack(_world, j);
							break;
						}
					}
				}
				destroyElement.parents.Clear();
			}
		}

		/// <summary>
		/// Убивает entity-ребёнка.
		/// Т.е. удаляет из всех родителей информацию о передаваемой entity.
		/// </summary>
		private void KillChild(in Entity entity)
		{
			ref var destroyElement = ref _killElementsArchetype.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (!destroyElement.parents.IsCreated)
				return;

			for (var i = 0; i < destroyElement.parents.Count; i++)
			{
				var parent = destroyElement.parents[_world, i];
				ref var parentElement = ref _killElementsArchetype.TryGetElement(parent, out var isParentExist);
				if (!isParentExist || !_entitiesStatePart.ptr->IsEntityExist(_world, parent))
					continue;

				for (var j = 0; j < parentElement.children.Count; j++)
				{
					if (parentElement.children[_world, j].id == entity.id)
					{
						parentElement.children.RemoveAtSwapBack(_world, j);
						break;
					}
				}
			}
			destroyElement.parents.Clear();
		}

		/// <summary>
		/// Убивает entity-родителя.
		/// Т.е. убивает всех детей, которые зависят от переданной entity.
		/// </summary>
		private void KillParent(in Entity entity)
		{
			ref var destroyElement = ref _killElementsArchetype.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (!destroyElement.children.IsCreated)
				return;
			for (var i = 0; i < destroyElement.children.Count; i++)
			{
				var child = destroyElement.children[_world, i];

				if (!_entitiesStatePart.ptr->IsEntityExist(_world, child))
					continue;

				_killRequestArchetype.GetElement(child, out var isChildExist);
				if (isChildExist)
					continue;

				KillParent(child);
			}
			destroyElement.children.Clear();
		}
	}
}
