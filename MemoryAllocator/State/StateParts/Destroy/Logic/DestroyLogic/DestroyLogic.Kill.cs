using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct DestroyLogic
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestKill(Entity entity)
		{
			E.ASSERT(IsAlive(entity), "Попытка запросить уничтожение entity, которая уже отправлена на уничтожение.");
			_killRequestSet.GetElement(entity);
		}

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

		public void AddKillParent(Entity child, Entity parent)
		{
			E.ASSERT(IsAlive(child));
			E.ASSERT(_entityStatePart.Value().IsEntityExist(_worldState, parent));

			if (_destroyRequestSet.HasElement(parent))
			{
				RequestKill(child);
				return;
			}

			ref var childElement = ref _killCallbackSet.GetElement(child);
			ref var parentElement = ref _killCallbackSet.GetElement(parent);

			if (!childElement.parents.IsCreated)
				childElement.parents = new MemList<Entity>(_worldState);
			if (!childElement.children.IsCreated)
				childElement.children = new MemList<Entity>(_worldState);

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
				if (!childElement.parents.IsCreated)
					childElement.parents = new MemList<Entity>(_worldState);
				childElement.parents.Add(_worldState, parent);
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
