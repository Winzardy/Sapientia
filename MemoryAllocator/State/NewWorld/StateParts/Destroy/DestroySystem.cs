namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe struct DestroySystem : IWorldSystem
	{
		private AllocatorId _allocatorId;
		public AllocatorId AllocatorId { get => _allocatorId; set => _allocatorId = value; }

		public void Update(float deltaTime)
		{
			var updater = new DestroyUpdater(_allocatorId.GetAllocatorPtr());
			updater.Update(deltaTime);
		}
	}
}
