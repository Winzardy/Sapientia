using System;

namespace Sapientia
{
	[Serializable]
	public struct UsageLimitData
	{
		public int usageCount;

		/// <see cref="DateTime.Ticks"/>
		public long lastUsageTimestamp;

		/// <see cref="DateTime.Ticks"/>
		public long firstUsageTimestamp;

		public int fullUsageCount;
	}
}
