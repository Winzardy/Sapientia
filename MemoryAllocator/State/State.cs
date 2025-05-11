using System;
using Sapientia.Data;
using Sapientia.Extensions;
#if UNITY_EDITOR
using Debug = UnityEngine.Debug;
#endif

namespace Sapientia.MemoryAllocator.State
{
	public struct State : IDisposable
	{
		public const int MAX_TICKS_PER_FRAME = 5;

		private WorldId _worldId;

		public WorldId WorldId => _worldId;
		public World World => _worldId.GetWorld();
		public bool IsValid => _worldId.IsValid();

		public State(WorldId worldId)
		{
			_worldId = worldId;
		}

		private SafePtr<WorldState> GetWorld()
		{
			return _worldId.GetWorld().GetServicePtr<WorldState>();
		}

		public void Start()
		{
			GetWorld().Value().Start();
		}

		public void Update(float deltaTime)
		{
			if (!IsValid)
				return;

			var updateStatePart = _worldId.GetLocalService<UpdateLocalStatePart>();
			if (!updateStatePart.CanUpdate())
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

				GetWorld().Value().Update(logicDeltaTime);
			}
		}

		public void LateUpdate()
		{
			if (!IsValid)
				return;
			var updateStatePart = _worldId.GetLocalService<UpdateLocalStatePart>();
			if (!updateStatePart.CanLateUpdate())
				return;

			GetWorld().Value().LateUpdate();
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			LocalStatePartService.Dispose(_worldId.GetWorld());
			GetWorld().Value().Dispose();
		}
	}
}
