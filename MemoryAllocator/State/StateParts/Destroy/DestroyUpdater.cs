using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	internal readonly unsafe ref struct DestroyUpdater
	{
		private readonly SafePtr<Allocator> _allocator;
		private readonly SafePtr<EntityStatePart> _entitiesStatePart;
		private readonly ArchetypeContext<DestroyRequest> _destroyRequestArchetype;
		private readonly ArchetypeContext<KillRequest> _killRequestArchetype;
		private readonly ArchetypeContext<DelayKillRequest> _delayKillRequestArchetype;
		private readonly ArchetypeContext<KillElement> _killElementsArchetype;

		public DestroyUpdater(SafePtr<Allocator> allocator)
		{
			_allocator = allocator;
			_entitiesStatePart = _allocator.GetServicePtr<EntityStatePart>();
			_destroyRequestArchetype = new (_allocator);
			_killRequestArchetype = new (_allocator);
			_delayKillRequestArchetype = new (_allocator);
			_killElementsArchetype = new (_allocator);
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

			_entitiesStatePart.ptr->DestroyEntities(_allocator, elementsToDestroy, count);
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

			var killRequests = _killRequestArchetype.GetRawElements();

			for (var i = 0; i < _killRequestArchetype.Count; i++)
			{
				var entity = killRequests[i].entity;

				ExecuteKillCallback(entity);
				KillChild(entity);
				KillParent(entity);

				_delayKillRequestArchetype.RemoveSwapBackElement(entity);
				_destroyRequestArchetype.GetElement(entity);
			}

			_killRequestArchetype.Clear();
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
					ref var killCallback = ref destroyElement.killCallbacks[_allocator, i];

					if (killCallback.callback.IsCreated)
					{
						killCallback.callback.EntityKilled(_allocator, _allocator, killCallback.target);
						killCallback.callback.Dispose(_allocator);
					}

					ref var targetElement = ref _killElementsArchetype.TryGetElement(killCallback.target, out var isTargetExist);
					if (!isTargetExist || !_entitiesStatePart.ptr->IsEntityExist(_allocator, killCallback.target))
						continue;
					if (targetElement.killCallbackHolders.IsCreated)
						continue;

					for (var j = 0; j < targetElement.killCallbackHolders.Count; j++)
					{
						if (targetElement.killCallbackHolders[_allocator, i].id == entity.id)
							targetElement.killCallbackHolders.RemoveAtSwapBack(_allocator, i);
					}
				}
				destroyElement.killCallbacks.Clear();
			}

			if (destroyElement.killCallbackHolders.IsCreated)
			{
				for (var i = 0; i < destroyElement.killCallbackHolders.Count; i++)
				{
					var holder = destroyElement.killCallbackHolders[_allocator, i];
					if (!_killElementsArchetype.HasElement(holder) || !_entitiesStatePart.ptr->IsEntityExist(_allocator, holder))
						continue;
					ref var holderElement = ref _killElementsArchetype.GetElement(holder);
					for (var j = 0; j < holderElement.killCallbacks.Count; j++)
					{
						if (holderElement.killCallbacks[_allocator, j].target.id == entity.id)
						{
							holderElement.killCallbacks[_allocator, j].callback.Dispose(_allocator);
							holderElement.killCallbacks.RemoveAtSwapBack(_allocator, j);
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
				var parent = destroyElement.parents[_allocator, i];
				ref var parentElement = ref _killElementsArchetype.TryGetElement(parent, out var isParentExist);
				if (!isParentExist || !_entitiesStatePart.ptr->IsEntityExist(_allocator, parent))
					continue;

				for (var j = 0; j < parentElement.children.Count; j++)
				{
					if (parentElement.children[_allocator, j].id == entity.id)
					{
						parentElement.children.RemoveAtSwapBack(_allocator, j);
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
				var child = destroyElement.children[_allocator, i];

				if (!_entitiesStatePart.ptr->IsEntityExist(child))
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
