using System;

namespace Sapientia
{
	[Serializable]
	public struct UsageLimitEntry
	{
		public const int INFINITY_USAGES = -1;

		/// <summary>
		/// Количество использований, если 0 - неограничено
		/// </summary>
		public int usageCount;

		/// <summary>
		/// Сброс между использованиями
		/// </summary>
		public ScheduleEntry reset;

		/// <summary>
		/// Полный сброса лимита
		/// </summary>
		public ScheduleEntry fullReset;
	}
}
