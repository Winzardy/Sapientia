namespace Sapientia.MemoryAllocator.State
{
	public struct DestroyStatePart : IWorldStatePart
	{
		public void Initialize(WorldState worldState, IndexedPtr self)
		{
			ComponentSet.RegisterComponentSet<DestroyElement>(worldState, 1024)
				.SetDestroyHandler<DestroyElementDestroyHandler>(worldState);
			ComponentSet.RegisterComponentSet<KillElement>(worldState, 2048)
				.SetDestroyHandler<KillElementDestroyHandler>(worldState);
			ComponentSet.RegisterComponentSet<KillRequest>(worldState, 256);
			ComponentSet.RegisterComponentSet<DelayKillRequest>(worldState, 64);
			ComponentSet.RegisterComponentSet<DestroyRequest>(worldState, 256);

			ComponentSet.RegisterComponentSet<AliveDuration>(worldState, 512);
			ComponentSet.RegisterComponentSet<AliveTimeDebt>(worldState, 512);
		}
	}
}
