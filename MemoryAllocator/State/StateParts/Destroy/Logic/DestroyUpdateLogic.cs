using System;
using Sapientia.Data;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct DestroyUpdateLogic : IInitializableService
	{
		private WorldState _worldState;

		private SafePtr<EntityStatePart> _entitiesStatePart;

		private SafePtr<DestroyLogic> _destroyLogic;

		private ComponentSetContext<DestroyRequest> _destroyRequestSet;
		private ComponentSetContext<DestroyComponent> _destroyComponentsSet;
		private ComponentSetContext<KillRequest> _killRequestSet;
		private ComponentSetContext<DelayKillRequest> _delayKillRequestSet;
		private ComponentSetContext<KillCallbackComponent> _killCallbackSet;

		void IInitializableService.Initialize(WorldState worldState)
		{
			_worldState = worldState;
			_entitiesStatePart = _worldState.GetServicePtr<EntityStatePart>();
			_destroyLogic = _worldState.GetOrCreateServicePtr<DestroyLogic>(ServiceType.NoState);
			_destroyRequestSet = new (_worldState);
			_destroyComponentsSet = new (_worldState);
			_killRequestSet = new (_worldState);
			_delayKillRequestSet = new (_worldState);
			_killCallbackSet = new (_worldState);
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
			var count = _destroyRequestSet.Count;
			if (count < 1)
				return;

			var destroyRequests = _destroyRequestSet.GetRawElements();
			var destroyRequestTempRaw = stackalloc ComponentSetElement<DestroyRequest>[count];
			var destroyRequestsTemp = new SafePtr<ComponentSetElement<DestroyRequest>>(destroyRequestTempRaw, count);

			MemoryExt.MemCopy<ComponentSetElement<DestroyRequest>>(destroyRequests, destroyRequestsTemp, count);

			for (var i = 0; i < count; i++)
			{
				var entity = destroyRequestsTemp[i].entity;

				DestroyChild(entity);
				DestroyParent(entity);
			}

			destroyRequests = _destroyRequestSet.GetRawElements();
			var elementsToDestroy = stackalloc Entity[_destroyRequestSet.Count];

			for (var i = 0; i < count; i++)
			{
				elementsToDestroy[i] = destroyRequests[i].entity;
			}

			_entitiesStatePart.Value().DestroyEntities(_worldState, elementsToDestroy, count);
		}

		/// <summary>
		/// Добавляет все entity, которые были помечены для убийства в список для дальшейшего уничтожения.
		/// Все эти entity живут ещё один тик.
		/// </summary>
		private void KillEntities(float deltaTime)
		{
			var delayKillRequests = _delayKillRequestSet.GetRawElements();

			for (var i = 0; i < _delayKillRequestSet.Count; i++)
			{
				ref var request = ref delayKillRequests[i];
				if (request.value.delay <= 0)
					_killRequestSet.GetElement(request.entity);
				else
					request.value.delay -= deltaTime;
			}

			var killRequestsCount = _killRequestSet.Count;
			if (killRequestsCount < 1)
				return;

			var killRequestsTemp = (Span<ComponentSetElement<KillRequest>>)stackalloc ComponentSetElement<KillRequest>[killRequestsCount];
			var killRequests = _killRequestSet.GetSpan();

			killRequests.CopyTo(killRequestsTemp);
			_killRequestSet.Clear();

			ref var destroyLogic = ref _destroyLogic.Value();
			for (var i = 0; i < killRequestsCount; i++)
			{
				var entity = killRequestsTemp[i].entity;

				// Выключаем entity перед убийством
				destroyLogic.Disable(entity);
				KillEntity(entity);

				_delayKillRequestSet.RemoveSwapBackElement(entity);
				_destroyRequestSet.GetElement(entity);
			}
		}

		/// <summary>
		/// Вызывает ивент, уведомляющий подписчиков об убийстве entity.
		/// Затем чистит список подписчиков.
		/// </summary>
		private void ExecuteKillCallback(in Entity entity)
		{
			ref var callbackComponent = ref _killCallbackSet.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (callbackComponent.killCallbacks.IsCreated)
			{
				for (var i = 0; i < callbackComponent.killCallbacks.Count; i++)
				{
					ref var killCallback = ref callbackComponent.killCallbacks[_worldState, i];

					if (killCallback.callback.IsCreated)
					{
						killCallback.callback.OnEntityKilled(_worldState, _worldState, killCallback.callbackReceiver);
						killCallback.callback.Dispose(_worldState);
					}
				}
				callbackComponent.killCallbacks.Clear();
			}

			if (callbackComponent.callbackTargets.IsCreated)
			{
				for (var i = 0; i < callbackComponent.callbackTargets.Count; i++)
				{
					var callbackTarget = callbackComponent.callbackTargets[_worldState, i];
					if (!_killCallbackSet.HasElement(callbackTarget) || !_entitiesStatePart.Value().IsEntityExist(_worldState, callbackTarget))
						continue;
					ref var targetCallbackComponent = ref _killCallbackSet.GetElement(callbackTarget);
					for (var j = 0; j < targetCallbackComponent.killCallbacks.Count; j++)
					{
						if (targetCallbackComponent.killCallbacks[_worldState, j].callbackReceiver == entity)
						{
							targetCallbackComponent.killCallbacks[_worldState, j].callback.Dispose(_worldState);
							targetCallbackComponent.killCallbacks.RemoveAtSwapBack(_worldState, j);
							break;
						}
					}
				}
				callbackComponent.callbackTargets.Clear();
			}
		}

		/// <summary>
		/// Убивает entity:
		/// - Удаляет из всех родителей информацию о передаваемой entity.
		/// - Запрашивает убийство всех детей на следующий тик.
		/// </summary>
		private void KillEntity(in Entity entity)
		{
			ExecuteKillCallback(entity);

			ref var callbackComponent = ref _killCallbackSet.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (callbackComponent.parents.IsCreated)
			{
				for (var i = 0; i < callbackComponent.parents.Count; i++)
				{
					var parent = callbackComponent.parents[_worldState, i];
					ref var parentCallbackComponent = ref _killCallbackSet.TryGetElement(parent, out var isParentExist);
					if (!isParentExist || !_entitiesStatePart.Value().IsEntityExist(_worldState, parent))
						continue;

					for (var j = 0; j < parentCallbackComponent.children.Count; j++)
					{
						if (parentCallbackComponent.children[_worldState, j] == entity)
						{
							parentCallbackComponent.children.RemoveAtSwapBack(_worldState, j);
							j--;
						}
					}
				}
				callbackComponent.parents.Clear();
			}

			RequestKillChildren(ref callbackComponent);
		}

		/// <summary>
		/// Убивает entity-родителя.
		/// Т.е. убивает всех детей, которые зависят от переданной entity.
		/// </summary>
		private void RequestKillChildren(ref KillCallbackComponent callbackComponent)
		{
			if (!callbackComponent.children.IsCreated)
				return;

			for (var i = 0; i < callbackComponent.children.Count; i++)
			{
				var child = callbackComponent.children[_worldState, i];

				if (!_entitiesStatePart.Value().IsEntityExist(_worldState, child))
					continue;

				_killRequestSet.GetElement(child, out var isChildExist);
				if (isChildExist)
					continue;

				ref var childCallbackComponent = ref _killCallbackSet.TryGetElement(child, out var isExist);
				if (isExist)
					RequestKillChildren(ref childCallbackComponent);
			}
			callbackComponent.children.Clear();
		}

		/// <summary>
		/// Уничтожает entity-ребёнка.
		/// Т.е. удаляет из всех родителей информацию о передаваемой entity.
		/// </summary>
		private void DestroyChild(in Entity entity)
		{
			ref var destroyComponent = ref _destroyComponentsSet.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (!destroyComponent.parents.IsCreated)
				return;

			for (var i = 0; i < destroyComponent.parents.Count; i++)
			{
				var parent = destroyComponent.parents[_worldState, i];
				ref var parentElement = ref _destroyComponentsSet.TryGetElement(parent, out var isParentExist);
				if (!isParentExist || !_entitiesStatePart.Value().IsEntityExist(_worldState, parent))
					continue;

				for (var j = 0; j < parentElement.children.Count; j++)
				{
					if (parentElement.children[_worldState, j] == entity)
					{
						parentElement.children.RemoveAtSwapBack(_worldState, j);
						j--;
					}
				}
			}
			destroyComponent.parents.Clear();
		}

		/// <summary>
		/// Уничтожает entity-родителя.
		/// Т.е. уничтожает всех детей, которые зависят от переданной entity.
		/// </summary>
		private void DestroyParent(in Entity entity)
		{
			ref var destroyComponent = ref _destroyComponentsSet.TryGetElement(entity, out var isExist);
			if (!isExist)
				return;

			if (!destroyComponent.children.IsCreated)
				return;
			for (var i = 0; i < destroyComponent.children.Count; i++)
			{
				var child = destroyComponent.children[_worldState, i];

				if (!_entitiesStatePart.Value().IsEntityExist(_worldState, child))
					continue;

				_destroyRequestSet.GetElement(child, out var isChildExist);
				if (isChildExist)
					continue;

				DestroyParent(child);
			}
			destroyComponent.children.Clear();
		}
	}
}
