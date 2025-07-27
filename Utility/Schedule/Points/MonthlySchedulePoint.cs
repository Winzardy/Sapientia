using System;

namespace Sapientia
{
	[Serializable]
	public struct MonthlySchedulePoint : ISchedulePoint
	{
		public SchedulePointKind Kind => SchedulePointKind.Yearly;
		public long code;

		long ISchedulePoint.Code => code;
	}
}
