using System;

namespace Sapientia
{
	[Serializable]
	public struct DailySchedulePoint : ISchedulePoint
	{
		public SchedulePointKind Kind => SchedulePointKind.Yearly;
		public int code;

		long ISchedulePoint.Code => code;
	}
}
