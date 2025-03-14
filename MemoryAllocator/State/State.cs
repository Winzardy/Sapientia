using System;
using System.Diagnostics;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.State
{
	public unsafe struct State : IDisposable
	{
		public const int MAX_TICKS_PER_FRAME = 5;

		private AllocatorId _allocatorId;

		public AllocatorId AllocatorId => _allocatorId;
		public Allocator* AllocatorPtr => _allocatorId.GetAllocatorPtr();
		public bool IsValid => _allocatorId.IsValid();

		public State(AllocatorId allocatorId)
		{
			_allocatorId = allocatorId;
		}

		private World* GetWorld()
		{
			return _allocatorId.GetAllocatorPtr()->GetServicePtr<World>();
		}

		public void Update(float deltaTime)
		{
			if (!IsValid)
				return;

			var updateStatePart = _allocatorId.GetLocalService<UpdateLocalStatePart>();
			if (!updateStatePart.CanUpdate())
				return;

			var tickTime = updateStatePart.stateUpdateData.tickTime;
			deltaTime *= updateStatePart.stateUpdateData.gameSpeed;

			updateStatePart.worldTimeDebt += deltaTime;

			if (tickTime <= 0f)
				return;

			var ticksToUpdate = (updateStatePart.worldTimeDebt / tickTime).FloorToInt_Positive();
			Debug.Assert(ticksToUpdate <= MAX_TICKS_PER_FRAME, $"{ticksToUpdate} ticks was scheduled in this frame.");

			if (ticksToUpdate > 0)
			{
				// Если тиков больше, чем максимально допустимо, то отбрасываем лишние
				ticksToUpdate = ticksToUpdate.Min(MAX_TICKS_PER_FRAME);

				var logicDeltaTime = ticksToUpdate * tickTime;
				updateStatePart.worldTimeDebt -= logicDeltaTime;

				GetWorld()->Update(logicDeltaTime);
			}
		}

		public void LateUpdate()
		{
			if (!IsValid)
				return;
			var updateStatePart = _allocatorId.GetLocalService<UpdateLocalStatePart>();
			if (!updateStatePart.CanLateUpdate())
				return;

			GetWorld()->LateUpdate();
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			LocalStatePartService.Dispose(_allocatorId.GetAllocatorPtr());
			GetWorld()->Dispose();
		}
	}
}
