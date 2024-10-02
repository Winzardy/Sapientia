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
			Archetype<KillElement>.RegisterArchetype(allocator, 512).SetDestroyHandler<KillElementDestroyHandler>();
			Archetype<KillRequest>.RegisterArchetype(allocator, 64);
			Archetype<DelayKillRequest>.RegisterArchetype(allocator, 64);
			Archetype<DestroyRequest>.RegisterArchetype(allocator, 64);
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

		public void EntityArrayDestroyed(Allocator* allocator, ArchetypeElement<KillElement>* element, uint count)
		{
			for (var i = 0u; i < count; i++)
			{
				element[i].value.children.Clear();
				element[i].value.parents.Clear();
				element[i].value.killCallbackHolders.Clear();
				foreach (KillCallback* component in element[i].value.killCallbacks.GetPtrEnumerable(allocator))
				{
					component->callback.Dispose(allocator);
				}
				element[i].value.killCallbacks.Clear();
			}
		}
	}
}
