using System;
using System.Diagnostics;
using Sapientia.Extensions;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe struct State : IDisposable
	{
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
			if (ticksToUpdate > 0)
			{
				var logicDeltaTime = ticksToUpdate * tickTime;
				updateStatePart.worldTimeDebt -= logicDeltaTime;

				GetWorld()->Update(logicDeltaTime);
			}
			Debug.Assert(ticksToUpdate <= 5, $"{ticksToUpdate} ticks was processed in this frame.");

			updateStatePart.scheduleLateGameUpdate = true;
		}

		public void LateUpdate()
		{
			if (!IsValid)
				return;
			var updateStatePart = _allocatorId.GetLocalService<UpdateLocalStatePart>();
			if (!updateStatePart.CanLateUpdate())
				return;
			updateStatePart.scheduleLateGameUpdate = false;

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
