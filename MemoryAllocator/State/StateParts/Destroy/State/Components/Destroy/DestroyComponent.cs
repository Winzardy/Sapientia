namespace Sapientia.MemoryAllocator.State
{
	public struct DestroyComponent : IComponent
	{
		public MemList<Entity> children;
		public MemList<Entity> parents;
	}

	public unsafe struct DestroyElementDestroyHandler : IElementDestroyHandler<DestroyComponent>
	{
		public void EntityPtrArrayDestroyed(WorldState worldState, ComponentSetElement<DestroyComponent>** elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i]->value;
				value.children.Dispose(worldState);
				value.parents.Dispose(worldState);
			}
		}

		public void EntityArrayDestroyed(WorldState worldState, ComponentSetElement<DestroyComponent>* elementsPtr, int count)
		{
			for (var i = 0; i < count; i++)
			{
				ref var value = ref elementsPtr[i].value;
				value.children.Dispose(worldState);
				value.parents.Dispose(worldState);
			}
		}
	}
}
