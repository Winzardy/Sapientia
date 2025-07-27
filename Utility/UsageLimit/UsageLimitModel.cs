using System;

namespace Sapientia
{
	[Serializable]
	public struct UsageLimitModel
	{
		public int usageCount;
		public long lastUsageTicks;
	}
}
