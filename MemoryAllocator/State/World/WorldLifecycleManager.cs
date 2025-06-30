using System;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.State;
#if UNITY_EDITOR
using Debug = UnityEngine.Debug;
#endif

namespace Sapientia.MemoryAllocator
{
	public struct WorldLifecycleManager : IDisposable
	{
		public const int MAX_TICKS_PER_FRAME = 5;

		private World _world;
		public bool IsValid => _world is { IsValid: true };

		public WorldLifecycleManager(World world)
		{
			_world = world;
		}

		public float GetResumeDelay()
		{
			if (!IsValid)
				return 0f;
			ref var updateStatePart = ref _world.worldState.GetUnmanagedLocalService<UpdateLocalStatePart>();
			return updateStatePart.stateUpdateData.resumeDelay;
		}

		public void ResumeSimulation()
		{
			if (!IsValid)
				return;
			ref var updateStatePart = ref _world.worldState.GetUnmanagedLocalService<UpdateLocalStatePart>();
			updateStatePart.ResumeSimulation();
		}

		public void PauseSimulation()
		{
			if (!IsValid)
				return;
			ref var updateStatePart = ref _world.worldState.GetUnmanagedLocalService<UpdateLocalStatePart>();
			updateStatePart.PauseSimulation();
		}

		public bool IsPaused()
		{
			var updateStatePart = _world.worldState.GetLocalService<UpdateLocalStatePart>();
			return updateStatePart.IsPaused();
		}

		public void Start()
		{
			if (!IsValid)
				return;
			_world.Start();
		}

		public void Update(float deltaTime)
		{
			if (!IsValid)
				return;

			ref var updateStatePart = ref _world.worldState.GetUnmanagedLocalService<UpdateLocalStatePart>();
			if (updateStatePart.IsPaused())
				return;

			var tickTime = updateStatePart.stateUpdateData.tickTime;

			if (tickTime <= 0f)
				return;

			deltaTime *= updateStatePart.stateUpdateData.gameSpeed;
			updateStatePart.worldTimeDebt += deltaTime;

			var ticksToUpdate = (updateStatePart.worldTimeDebt / tickTime).FloorToInt_Positive();
#if UNITY_EDITOR
			if (ticksToUpdate > MAX_TICKS_PER_FRAME)
				Debug.LogWarning($"{ticksToUpdate} ticks was scheduled in this frame.");
#endif

			if (ticksToUpdate > 0)
			{
				var logicDeltaTime = ticksToUpdate * tickTime;

				// Если тиков больше, чем максимально допустимо, то отбрасываем лишние
				if (MAX_TICKS_PER_FRAME < ticksToUpdate)
				{
					ticksToUpdate = MAX_TICKS_PER_FRAME;
					logicDeltaTime = ticksToUpdate * tickTime;

					updateStatePart.worldTimeDebt = 0;
				}
				else
					updateStatePart.worldTimeDebt -= logicDeltaTime;

				_world.Update(logicDeltaTime);
			}
		}

		public void LateUpdate()
		{
			if (!IsValid)
				return;

			ref var updateStatePart = ref _world.worldState.GetUnmanagedLocalService<UpdateLocalStatePart>();
			if (!updateStatePart.ShouldLateUpdate())
				return;

			_world.LateUpdate();
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			_world.Dispose();
		}
	}
}
