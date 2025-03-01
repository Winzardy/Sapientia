using System;

namespace Sapientia.MemoryAllocator.State.NewWorld
{
	[Serializable]
	public struct StateUpdateData
	{
		public float gameSpeed;
		public float tickTime;
		public float resumeDelay;
	}
}
