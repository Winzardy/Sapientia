using System;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public unsafe struct State : IDisposable
	{
		private AllocatorId _allocatorId;

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
			GetWorld()->Update(deltaTime);
		}

		public void LateUpdate()
		{
			GetWorld()->LateUpdate();
		}

		public void Dispose()
		{
			LocalStatePartService.Dispose(_allocatorId.GetAllocatorPtr());
			GetWorld()->Dispose();
		}
	}
}
