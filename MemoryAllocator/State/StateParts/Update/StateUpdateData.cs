using System;

namespace Sapientia.MemoryAllocator.State
{
	[Serializable]
	public struct StateUpdateData
	{
		public float gameSpeed;
		public float tickTime;
		public float resumeDelay;
	}
}
