namespace Sapientia.MemoryAllocator.State
{
	public struct AliveDurationSystem : IWorldSystem
	{
		public void Update(WorldState worldState, IndexedPtr self, float deltaTime)
		{
			var aliveDurationSet = new ComponentSetContext<AliveDuration>(worldState);
			ref var destroySet = ref worldState.GetOrCreateService<DestroyLogic>(ServiceType.NoState);

			foreach (ref var element in aliveDurationSet.GetEnumerable())
			{
				var entity = element.entity;

				if (destroySet.HasDestroyRequest(entity))
					continue;
				ref var value = ref element.value;

				var timeDebt = destroySet.GetTimeDebt(entity);
				value.currentDuration += deltaTime + timeDebt;

				if (!value.destroyDuration.TryGetValue(out var destroyDuration))
					continue;
				if (destroyDuration > value.currentDuration)
					continue;

				value.currentDuration = destroyDuration;
				if (!destroySet.HasKillRequest(entity))
					destroySet.RequestKill(entity);

				if (value.currentDuration > destroyDuration)
					destroySet.SetTimeDebt(entity, value.currentDuration - destroyDuration);
			}
		}
	}

}
