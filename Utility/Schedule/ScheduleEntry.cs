using System;

namespace Sapientia
{
	/// <summary>
	/// Временные точки, с помощью которых можно создать расписание для события. Например сброс счетчиков
	/// </summary>
	[Serializable]
	public struct ScheduleEntry
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
