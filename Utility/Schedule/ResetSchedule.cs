using System;

namespace Sapientia
{
	[Serializable]
	public struct ResetSchedule
	{
		public SchedulePoint[] points;
	}

	[Serializable]
	public struct SchedulePoint : ISchedulePoint
	{
		/// <inheritdoc cref="ISchedulePoint.Code"/>
		public long code;

		public long Code => code;
	}
}
