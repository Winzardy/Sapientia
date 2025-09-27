using System;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator.State
{
	internal readonly unsafe ref struct DestroyUpdater
	{
		private readonly WorldState _worldState;
		private readonly SafePtr<EntityStatePart> _entitiesStatePart;
		private readonly ComponentSetContext<DestroyRequest> _destroyRequestArchetype;
		private readonly ComponentSetContext<DestroyElement> _destroyElementsArchetype;
		private readonly ComponentSetContext<KillRequest> _killRequestArchetype;
		private readonly ComponentSetContext<DelayKillRequest> _delayKillRequestArchetype;
		private readonly ComponentSetContext<KillElement> _killElementsArchetype;

		public DestroyUpdater(WorldState worldState)
		{
			_worldState = worldState;
			_entitiesStatePart = _worldState.GetServicePtr<EntityStatePart>();
			_destroyRequestArchetype = new (_worldState);
			_destroyElementsArchetype = new (_worldState);
			_killRequestArchetype = new (_worldState);
			_delayKillRequestArchetype = new (_worldState);
			_killElementsArchetype = new (_worldState);
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
			var destroyRequestTempRaw = stackalloc ComponentSetElement<DestroyRequest>[count];
			var destroyRequestsTemp = new SafePtr<ComponentSetElement<DestroyRequest>>(destroyRequestTempRaw, count);

			MemoryExt.MemCopy<ComponentSetElement<DestroyRequest>>(destroyRequests, destroyRequestsTemp, count);

			for (var i = 0; i < count; i++)
			{
				var entity = destroyRequestsTemp[i].entity;

				DestroyChild(entity);
				DestroyParent(entity);
			}

			destroyRequests = _destroyRequestArchetype.GetRawElements();
			var elementsToDestroy = stackalloc Entity[_destroyRequestArchetype.Count];

			for (var i = 0; i < count; i++)
			{
				elementsToDestroy[i] = destroyRequests[i].entity;
			}

			_entitiesStatePart.ptr->DestroyEntities(_worldState, elementsToDestroy, count);
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
					_killRequestArchetype.GetElement(request.entity);
				else
					request.value.delay -= deltaTime;
			}

			var killRequestsCount = _killRequestArchetype.Count;
			if (killRequestsCount < 1)
				return;

			var killRequestsTemp = (Span<ComponentSetElement<KillRequest>>)stackalloc ComponentSetElement<KillRequest>[killRequestsCount];
			var killRequests = _killRequestArchetype.GetSpan();

			killRequests.CopyTo(killRequestsTemp);
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

		/// <summary>
		/// Вызывает ивент, уведомляющий подписчиков об убийстве entity.
		/// Затем чистит список подписчиков.
		/// </summary>
		private void ExecuteKillCallback(in Entity entity)
		{
			ref var killElement = ref _killElementsArchetype.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (killElement.killCallbacks.IsCreated)
			{
				for (var i = 0; i < killElement.killCallbacks.Count; i++)
				{
					ref var killCallback = ref killElement.killCallbacks[_worldState, i];

					if (killCallback.callback.IsCreated)
					{
						killCallback.callback.EntityKilled(_worldState, _worldState, killCallback.target);
						killCallback.callback.Dispose(_worldState);
					}

					ref var targetElement = ref _killElementsArchetype.TryGetElement(killCallback.target, out var isTargetExist);
					if (!isTargetExist || !_entitiesStatePart.ptr->IsEntityExist(_worldState, killCallback.target))
						continue;
					if (targetElement.killCallbackHolders.IsCreated)
						continue;

					for (var j = 0; j < targetElement.killCallbackHolders.Count; j++)
					{
						if (targetElement.killCallbackHolders[_worldState, i].id == entity.id)
							targetElement.killCallbackHolders.RemoveAtSwapBack(_worldState, i);
					}
				}
				killElement.killCallbacks.Clear();
			}

			if (killElement.killCallbackHolders.IsCreated)
			{
				for (var i = 0; i < killElement.killCallbackHolders.Count; i++)
				{
					var holder = killElement.killCallbackHolders[_worldState, i];
					if (!_killElementsArchetype.HasElement(holder) || !_entitiesStatePart.ptr->IsEntityExist(_worldState, holder))
						continue;
					ref var holderElement = ref _killElementsArchetype.GetElement(holder);
					for (var j = 0; j < holderElement.killCallbacks.Count; j++)
					{
						if (holderElement.killCallbacks[_worldState, j].target.id == entity.id)
						{
							holderElement.killCallbacks[_worldState, j].callback.Dispose(_worldState);
							holderElement.killCallbacks.RemoveAtSwapBack(_worldState, j);
							break;
						}
					}
				}
				killElement.parents.Clear();
			}
		}

		/// <summary>
		/// Убивает entity-ребёнка.
		/// Т.е. удаляет из всех родителей информацию о передаваемой entity.
		/// </summary>
		private void KillChild(in Entity entity)
		{
			ref var killElement = ref _killElementsArchetype.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (!killElement.parents.IsCreated)
				return;

			for (var i = 0; i < killElement.parents.Count; i++)
			{
				var parent = killElement.parents[_worldState, i];
				ref var parentElement = ref _killElementsArchetype.TryGetElement(parent, out var isParentExist);
				if (!isParentExist || !_entitiesStatePart.ptr->IsEntityExist(_worldState, parent))
					continue;

				for (var j = 0; j < parentElement.children.Count; j++)
				{
					if (parentElement.children[_worldState, j].id == entity.id)
					{
						parentElement.children.RemoveAtSwapBack(_worldState, j);
						break;
					}
				}
			}
			killElement.parents.Clear();
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
				var child = destroyElement.children[_worldState, i];

				if (!_entitiesStatePart.ptr->IsEntityExist(_worldState, child))
					continue;

				_killRequestArchetype.GetElement(child, out var isChildExist);
				if (isChildExist)
					continue;

				KillParent(child);
			}
			destroyElement.children.Clear();
		}

		/// <summary>
		/// Уничтожает entity-ребёнка.
		/// Т.е. удаляет из всех родителей информацию о передаваемой entity.
		/// </summary>
		private void DestroyChild(in Entity entity)
		{
			ref var destroyElement = ref _destroyElementsArchetype.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (!destroyElement.parents.IsCreated)
				return;

			for (var i = 0; i < destroyElement.parents.Count; i++)
			{
				var parent = destroyElement.parents[_worldState, i];
				ref var parentElement = ref _destroyElementsArchetype.TryGetElement(parent, out var isParentExist);
				if (!isParentExist || !_entitiesStatePart.ptr->IsEntityExist(_worldState, parent))
					continue;

				for (var j = 0; j < parentElement.children.Count; j++)
				{
					if (parentElement.children[_worldState, j].id == entity.id)
					{
						parentElement.children.RemoveAtSwapBack(_worldState, j);
						break;
					}
				}
			}
			destroyElement.parents.Clear();
		}

		/// <summary>
		/// Уничтожает entity-родителя.
		/// Т.е. уничтожает всех детей, которые зависят от переданной entity.
		/// </summary>
		private void DestroyParent(in Entity entity)
		{
			ref var destroyElement = ref _destroyElementsArchetype.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (!destroyElement.children.IsCreated)
				return;
			for (var i = 0; i < destroyElement.children.Count; i++)
			{
				var child = destroyElement.children[_worldState, i];

				if (!_entitiesStatePart.ptr->IsEntityExist(_worldState, child))
					continue;

				_destroyRequestArchetype.GetElement(child, out var isChildExist);
				if (isChildExist)
					continue;

				DestroyParent(child);
			}
			destroyElement.children.Clear();
		}
	}
}
