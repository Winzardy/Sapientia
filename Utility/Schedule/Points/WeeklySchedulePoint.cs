using System;

namespace Sapientia
{
	[Serializable]
	public struct WeeklySchedulePoint : ISchedulePoint
	{
		public SchedulePointKind Kind => SchedulePointKind.Yearly;
		public long code;

		long ISchedulePoint.Code => code;
	}
}
