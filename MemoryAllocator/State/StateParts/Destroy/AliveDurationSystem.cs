namespace Sapientia.MemoryAllocator.State
{
	public struct AliveDurationSystem : IWorldSystem
	{
		public void Update(WorldState worldState, IndexedPtr self, float deltaTime)
		{
			var destroyContext = new DestroyLogic(worldState);

			foreach (ref var element in destroyContext.aliveDurationContext.GetEnumerable())
			{
				var entity = element.entity;

				if (destroyContext.HasDestroyRequest(entity))
					continue;
				ref var value = ref element.value;

				var timeDebt = destroyContext.GetTimeDebt(entity);
				value.currentDuration += deltaTime + timeDebt;

				if (!value.destroyDuration.TryGetValue(out var destroyDuration))
					continue;
				if (destroyDuration > value.currentDuration)
					continue;

				value.currentDuration = destroyDuration;
				if (!destroyContext.HasKillRequest(entity))
					destroyContext.RequestKill(entity);

				if (value.currentDuration > destroyDuration)
					destroyContext.SetTimeDebt(entity, value.currentDuration - destroyDuration);
			}
		}
	}

}
