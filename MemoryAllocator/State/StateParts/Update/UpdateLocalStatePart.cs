namespace Sapientia.MemoryAllocator.State
{
	public struct UpdateLocalStatePart : IWorldUnmanagedLocalStatePart
	{
		public StateUpdateData stateUpdateData;

		public float worldTimeDebt;

		private int _pauseCount;

		public UpdateLocalStatePart(StateUpdateData stateUpdateData)
		{
			this.stateUpdateData = stateUpdateData;
			worldTimeDebt = 0;
			_pauseCount = 0;
		}

		public void ResumeSimulation()
		{
			if (_pauseCount > 0)
				_pauseCount--;
		}

		public void PauseSimulation()
		{
			_pauseCount++;
		}

		public bool CanUpdate()
		{
			return _pauseCount == 0;
		}

		public bool CanLateUpdate()
		{
			return true;
		}
	}
}
