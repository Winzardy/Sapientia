using System;

namespace Sapientia
{
	[Serializable]
	public struct UsageLimitEntry
	{
		/// <summary>
		/// Количество использований, если 0 - неограничено
		/// </summary>
		public int usageCount;

		/// <summary>
		/// Сброс между использованиями
		/// </summary>
		public ResetSchedule reset;

		/// <summary>
		/// Полный сброса лимита
		/// </summary>
		public ResetSchedule fullReset;
	}
}
