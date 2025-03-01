using System.Diagnostics;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	public class UpdateLocalStatePart : IWoldLocalStatePart
	{
		public StateUpdateData stateUpdateData;

		public float worldTimeDebt = 0;
		public bool scheduleLateGameUpdate = false;

		private int _pauseCount = 0;

		public UpdateLocalStatePart(StateUpdateData stateUpdateData)
		{
			this.stateUpdateData = stateUpdateData;
		}

		public void ResumeSimulation()
		{
			Debug.Assert(_pauseCount > 0);
			_pauseCount--;
		}

		public void PauseSimulation()
		{
			Debug.Assert(_pauseCount >= 0);
			_pauseCount++;
		}

		public bool CanUpdate()
		{
			return _pauseCount == 0;
		}

		public bool CanLateUpdate()
		{
			return scheduleLateGameUpdate;
		}
	}
}
