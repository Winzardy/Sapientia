using System;
using Sapientia.Data;
using Sapientia.Extensions;
#if UNITY_EDITOR
using Debug = UnityEngine.Debug;
#endif

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct State : IDisposable
	{
		public const int MAX_TICKS_PER_FRAME = 5;

		private AllocatorId _allocatorId;

		public AllocatorId AllocatorId => _allocatorId;
		public Allocator Allocator => _allocatorId.GetAllocator();
		public bool IsValid => _allocatorId.IsValid();

		public State(AllocatorId allocatorId)
		{
			_allocatorId = allocatorId;
		}

		private SafePtr<WorldState> GetWorld()
		{
			return _allocatorId.GetAllocator().GetServicePtr<WorldState>();
		}

		public void Start()
		{
			GetWorld().Value().Start();
		}

		public void Update(float deltaTime)
		{
			if (!IsValid)
				return;

			var updateStatePart = _allocatorId.GetLocalService<UpdateLocalStatePart>();
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
			var updateStatePart = _allocatorId.GetLocalService<UpdateLocalStatePart>();
			if (!updateStatePart.CanLateUpdate())
				return;

			GetWorld().Value().LateUpdate();
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			LocalStatePartService.Dispose(_allocatorId.GetAllocator());
			GetWorld().Value().Dispose();
		}
	}
}
