using System;

namespace Sapientia
{
	[Serializable]
	public struct YearlySchedulePoint : ISchedulePoint
	{
		public SchedulePointKind Kind => SchedulePointKind.Yearly;
		public long code;

		long ISchedulePoint.Code => code;
	}
}
