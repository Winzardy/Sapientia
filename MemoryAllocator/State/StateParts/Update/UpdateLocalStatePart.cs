namespace Sapientia.MemoryAllocator.State
{
	public class UpdateLocalStatePart : IWoldLocalStatePart
	{
		public StateUpdateData stateUpdateData;

		public float worldTimeDebt = 0;

		private int _pauseCount = 0;

		public UpdateLocalStatePart(StateUpdateData stateUpdateData)
		{
			this.stateUpdateData = stateUpdateData;
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
