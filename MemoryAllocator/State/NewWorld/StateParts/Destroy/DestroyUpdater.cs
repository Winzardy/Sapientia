using Sapientia.Extensions;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	internal readonly unsafe ref struct DestroyUpdater
	{
		private readonly Allocator* _allocator;
		private readonly EntitiesStatePart* _entitiesStatePart;
		private readonly Archetype<DestroyRequest>* _destroyRequestArchetype;
		private readonly Archetype<KillRequest>* _killRequestArchetype;
		private readonly Archetype<DelayKillRequest>* _delayKillRequestArchetype;
		private readonly Archetype<KillElement>* _killElementsArchetype;

		public DestroyUpdater(Allocator* allocator)
		{
			_allocator = allocator;
			_entitiesStatePart = _allocator->GetServicePtr<EntitiesStatePart>();
			_destroyRequestArchetype = _allocator->GetArchetypePtr<DestroyRequest>();
			_killRequestArchetype = _allocator->GetArchetypePtr<KillRequest>();
			_delayKillRequestArchetype = _allocator->GetArchetypePtr<DelayKillRequest>();
			_killElementsArchetype = _allocator->GetArchetypePtr<KillElement>();
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
			var count = _destroyRequestArchetype->Count;
			if (count < 1)
				return;

			var destroyRequests = _destroyRequestArchetype->GetRawElements();
			var elementsToDestroy = stackalloc ArchetypeElement<DestroyRequest>[(int)count];

			MemoryExt.MemCopy(destroyRequests, elementsToDestroy, TSize<ArchetypeElement<DestroyRequest>>.uSize * count);
			_destroyRequestArchetype->ClearFast();

			for (var i = 0; i < count; i++)
			{
				var entity = elementsToDestroy[i].entity;
				_entitiesStatePart->DestroyEntity(entity);
			}
		}

		/// <summary>
		/// Добавляет все entity, которые были помечены для убийства в список для дальшейшего уничтожения.
		/// Все эти entity живут ещё один тик.
		/// </summary>
		private void KillEntities(float deltaTime)
		{
			var delayKillRequests = _delayKillRequestArchetype->GetRawElements();

			for (var i = 0; i < _delayKillRequestArchetype->Count; i++)
			{
				ref var request = ref delayKillRequests[i];
				if (request.value.delay <= 0)
					_delayKillRequestArchetype->GetElement(request.entity);
				else
					request.value.delay -= deltaTime;
			}

			var killRequests = _killRequestArchetype->GetRawElements();

			for (var i = 0; i < _killRequestArchetype->Count; i++)
			{
				var entity = killRequests[i].entity;

				ExecuteKillCallback(entity);
				KillChild(entity);
				KillParent(entity);

				_delayKillRequestArchetype->RemoveSwapBackElement(entity);
				_destroyRequestArchetype->GetElement(entity);
			}

			_killRequestArchetype->Clear();
		}

		/// <summary>
		/// Вызывает ивент, уведомляющий подписчиков об убийстве entity.
		/// Затем чистит список подписчиков.
		/// </summary>
		private void ExecuteKillCallback(in Entity entity)
		{
			if (!_killElementsArchetype->HasElement(entity))
				return;
			ref var destroyElement = ref _killElementsArchetype->GetElement(entity);

			if (destroyElement.killCallbacks.IsCreated)
			{
				for (var i = 0; i < destroyElement.killCallbacks.Count; i++)
				{
					ref var killCallback = ref destroyElement.killCallbacks[_allocator, i];

					if (killCallback.callback.IsCreated)
					{
						killCallback.callback.EntityKilled(killCallback.target);
						killCallback.callback.Dispose(_allocator);
					}

					if (!_killElementsArchetype->HasElement(killCallback.target) || !_entitiesStatePart->IsEntityAlive(_allocator, killCallback.target))
						continue;
					ref var targetElement = ref _killElementsArchetype->GetElement(killCallback.target);
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
					if (!_killElementsArchetype->HasElement(holder) || !_entitiesStatePart->IsEntityAlive(_allocator, holder))
						continue;
					ref var holderElement = ref _killElementsArchetype->GetElement(holder);
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
			if (!_killElementsArchetype->HasElement(entity))
				return;
			ref var destroyElement = ref _killElementsArchetype->GetElement(entity);

			if (!destroyElement.parents.IsCreated)
				return;

			for (var i = 0; i < destroyElement.parents.Count; i++)
			{
				var parent = destroyElement.parents[_allocator, i];
				if (!_killElementsArchetype->HasElement(parent) || !_entitiesStatePart->IsEntityAlive(_allocator, parent))
					continue;
				ref var parentElement = ref _killElementsArchetype->GetElement(_allocator, parent);
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
			if (!_killElementsArchetype->HasElement(entity))
				return;
			ref var destroyElement = ref _killElementsArchetype->GetElement(entity);

			if (!destroyElement.children.IsCreated)
				return;
			for (var i = 0; i < destroyElement.children.Count; i++)
			{
				var child = destroyElement.children[_allocator, i];
				if (_killRequestArchetype->HasElement(child) || !_entitiesStatePart->IsEntityAlive(child))
					continue;
				_killRequestArchetype->GetElement(child);
				KillParent(child);
			}
			destroyElement.children.Clear();
		}
	}
}
