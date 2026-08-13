using System;

namespace Sapientia
{
	/// <summary>
	/// Временные точки, с помощью которых можно создать расписание для события. Например сброс счетчиков
	/// </summary>
	[Serializable]
	public struct ScheduleScheme
	{
		public SchedulePoint[] points;

		/// <summary>
		/// Длительности окон в секундах, параллельно <see cref="points"/>: durations[i] превращает
		/// точку i из момента в отрезок <c>[дата, дата + durations[i])</c>. <c>null</c>, нулевое
		/// значение или индекс за пределами массива — точка остаётся моментом<br/>
		/// Диапазон задаётся началом и длиной, а не парой точек «начало/конец»: пара правил не
		/// спаривается однозначно и разваливается на переходе через неделю или год
		/// </summary>
		/// <example>
		/// «пн–пт» = <see cref="SchedulePointKind.Weekly"/> понедельник 00:00 + 5 дней<br/>
		/// «каждый день 20:00–23:00» = <see cref="SchedulePointKind.Daily"/> 20:00 + 3 часа<br/>
		/// «с пт 22:00 до пн 06:00» = <see cref="SchedulePointKind.Weekly"/> пятница 22:00 + 3 дня 8 часов
		/// </example>
		/// <remarks>
		/// Отдельный массив, а не поле в <see cref="SchedulePoint"/>: окна нужны единицам расписаний
		/// (см. ScheduleWindowAttribute), и раздувать каждую точку всюду ради них незачем.
		/// У большинства схем здесь просто null
		/// </remarks>
		public long[] durations;
	}

	[Serializable]
	public struct SchedulePoint : ISchedulePoint
	{
		/// <inheritdoc cref="ISchedulePoint.Code"/>
		public long code;

		public long Code => code;
	}
}
