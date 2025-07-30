using System;

namespace Sapientia
{
	[Serializable]
	public struct UsageLimitModel
	{
		public int usageCount;

		/// <see cref="DateTime.Ticks"/>
		public long lastUsageDate;

		/// <see cref="DateTime.Ticks"/>
		public long firstUsageDate;

		public int fullUsageCount;
	}
}
