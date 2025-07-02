using Sapientia.MemoryAllocator.State;

namespace Sapientia.MemoryAllocator
{
	public abstract class EntityWorldBuilder : WorldBuilder
	{
		public int EntitiesCapacity { get; protected set; }

		public EntityWorldBuilder(StateUpdateData stateUpdateData, int entitiesCapacity) : base(stateUpdateData)
		{
			EntitiesCapacity = entitiesCapacity;
		}

		protected override void AddStateParts()
		{
			base.AddStateParts();

			AddStatePart(new EntityStatePart(EntitiesCapacity));
			AddStatePart<DestroyStatePart>();
		}
	}
}
