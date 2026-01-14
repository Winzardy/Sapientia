using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct DestroyLogic
	{
		/// <summary>
		/// Вызывает <see cref="RequestKill(Entity)"/> для группы <see cref="Entity"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestKill(Span<Entity> entities)
		{
			foreach (var entity in entities)
			{
				RequestKill(entity);
			}
		}

		/// <summary>
		/// Уничтожает <see cref="Entity"/> в конце следующего апдейта мира.
		/// <see cref="Entity"/> будет существовать ещё один тик, все события, связанные с <see cref="DestroyRequest"/>
		/// и <see cref="DestroyComponent"/> также будут обработаны в конце следующего тика.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestKill(Entity entity)
		{
			E.ASSERT(IsAlive(entity), "Попытка запросить уничтожение entity, которая уже отправлена на уничтожение.");
			_killRequestSet.GetElement(entity);
		}

		/// <summary>
		/// Вызывает <see cref="RequestKill(Entity)"/> для <see cref="Entity"/> через <see cref="delay"/> времени.
		/// </summary>
		public void RequestKill(Entity entity, float delay)
		{
			E.ASSERT(IsAlive(entity), "Попытка запросить уничтожение entity, которая уже отправлена на уничтожение.");
			ref var request = ref _delayKillRequestSet.GetElement(entity, out var isExist);
			if (!isExist || request.delay > delay)
				request.delay = delay;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasKillRequest(Entity entity)
		{
			return _killRequestSet.HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasDestroyOrKillRequest(Entity entity)
		{
			return _killRequestSet.HasElement(entity) || _destroyRequestSet.HasElement(entity);
		}

		public void AddKillParent(Entity child, Entity parent, bool duplicateCheck = false)
		{
			E.ASSERT(IsAlive(child));
			E.ASSERT(IsExist(parent));

			if (_destroyRequestSet.HasElement(parent))
			{
				RequestKill(child);
				return;
			}

			ref var childElement = ref _killCallbackSet.GetElement(child);
			if (duplicateCheck && childElement.parents.Contains(_worldState, parent))
				return;
			ref var parentElement = ref _killCallbackSet.GetElement(parent);

			childElement.parents.Add(_worldState, parent);
			parentElement.children.Add(_worldState, child);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddKillChild(Entity parent, Entity child)
		{
			AddKillParent(child, parent);
		}

		public void AddKillChildren(Entity parent, MemListEnumerable<Entity> children)
		{
			E.ASSERT(IsAlive(parent));

			ref var parentElement = ref _killCallbackSet.GetElement(parent);

			if (!parentElement.children.IsCreated)
				parentElement.children = new MemList<Entity>(_worldState);
			parentElement.children.AddRange(_worldState, children);

			foreach (var child in children)
			{
				E.ASSERT(IsAlive(child));
				ref var childElement = ref _killCallbackSet.GetElement(child);
				childElement.parents.Add(_worldState, parent);
			}
		}

		public bool HasKillParent(Entity child, Entity parent)
		{
			ref var childComponent = ref _killCallbackSet.TryGetElement(child, out var success);
			if (!success)
				return false;

			return childComponent.parents.Contains(_worldState, parent);
		}

		public bool HasKillCallback(Entity child)
		{
			return _killCallbackSet.HasElement(child);
		}

		public void RemoveKillParents(Entity child)
		{
			ref var childComponent = ref _killCallbackSet.TryGetElement(child, out var success);
			if (!success)
				return;

			foreach (var parent in childComponent.parents.GetEnumerable(_worldState))
			{
				ref var parentComponent = ref _killCallbackSet.GetElement(parent);
				parentComponent.children.RemoveSwapBack(_worldState, child);
			}
		}

		public void AddKillCallback<T>(Entity target, Entity callbackReceiver, in T callback = default) where T: unmanaged, IKillSubscriber
		{
			ref var targetComponent = ref _killCallbackSet.GetElement(target);

			targetComponent.killCallbacks.Add(_worldState, new Callback<IKillSubscriberProxy>
			{
				callback = ProxyPtr<IKillSubscriberProxy>.Create(_worldState, callback),
				callbackReceiver = callbackReceiver,
			});

			ref var receiverComponent = ref _killCallbackSet.GetElement(callbackReceiver);
			receiverComponent.callbackTargets.Add(_worldState, target);
		}
	}
}
