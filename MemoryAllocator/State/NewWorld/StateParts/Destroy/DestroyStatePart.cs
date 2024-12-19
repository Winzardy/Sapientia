using Sapientia.MemoryAllocator.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	[InterfaceProxy]
	public interface IKillSubscriber
	{
		public void EntityKilled(in Entity entity);
	}

	public struct KillCallback
	{
		public Entity target;
		public ProxyPtr<IKillSubscriberProxy> callback;
	}

	public struct KillElement : IComponent
	{
		public List<Entity> children;
		public List<Entity> parents;

		public List<Entity> killCallbackHolders;
		public List<KillCallback> killCallbacks;
	}

	public struct KillRequest : IComponent {}

	public struct DelayKillRequest : IComponent
	{
		public float delay;
	}

	public struct DestroyRequest : IComponent {}

	public struct DestroyStatePart : IWorldStatePart
	{
		public unsafe void Initialize(Allocator* allocator, IndexedPtr statePartPtr)
		{
			Archetype.RegisterArchetype<KillElement>(allocator, 512).SetDestroyHandler<KillElementDestroyHandler>();
			Archetype.RegisterArchetype<KillRequest>(allocator, 64);
			Archetype.RegisterArchetype<DelayKillRequest>(allocator, 64);
			Archetype.RegisterArchetype<DestroyRequest>(allocator, 64);
		}
	}

	public unsafe struct KillElementDestroyHandler : IElementDestroyHandler<KillElement>
	{
		public void EntityDestroyed(Allocator* allocator, ref ArchetypeElement<KillElement> element)
		{
			element.value.children.Clear();
			element.value.parents.Clear();
			element.value.killCallbackHolders.Clear();
			element.value.killCallbacks.Clear();
		}

		public void EntityArrayDestroyed(Allocator* allocator, ArchetypeElement<KillElement>* elementsPtr, int count)
		{
			for (var i = 0u; i < count; i++)
			{
				elementsPtr[i].value.children.Clear();
				elementsPtr[i].value.parents.Clear();
				elementsPtr[i].value.killCallbackHolders.Clear();
				foreach (KillCallback* component in elementsPtr[i].value.killCallbacks.GetPtrEnumerable(allocator))
				{
					component->callback.Dispose(allocator);
				}
				elementsPtr[i].value.killCallbacks.Clear();
			}
		}
	}
}
