namespace Sapientia.MemoryAllocator.State
{
	public struct DestroyStatePart : IWorldStatePart
	{
		public void Initialize(WorldState worldState, IndexedPtr self)
		{
			Archetype.RegisterArchetype<KillElement>(worldState, 2048).SetDestroyHandler<KillElementDestroyHandler>(worldState);
			Archetype.RegisterArchetype<KillRequest>(worldState, 256);
			Archetype.RegisterArchetype<DelayKillRequest>(worldState, 64);
			Archetype.RegisterArchetype<DestroyRequest>(worldState, 256);

			Archetype.RegisterArchetype<AliveDuration>(worldState, 512);
			Archetype.RegisterArchetype<AliveTimeDebt>(worldState, 512);
		}
	}
}
