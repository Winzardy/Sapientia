namespace Sapientia
{
	public interface ISchedulePoint
	{
		public const long TYPE_OFFSET = 10L;

		/// <summary>
		/// Закодированное время, декодирование зависит от типа:<br/>
		/// - <b>Interval</b> — интервал в секундах с момента запуска<br/>
		/// - <b>Daily</b> — секунда от начала дня (0–86399)<br/>
		/// - <b>Weekly</b> — день недели и секунда от начала дня<br/>
		///     Формула: <c>day * 100000 + seconds</c><br/>
		/// - <b>Monthly</b> — день месяца и секунда от начала дня<br/>
		///     Формула: <c>day * 100000 + seconds</c><br/>
		/// - <b>Yearly</b> — месяц, день месяца и секунда от начала дня<br/>
		///     Формула: <c>month * 10000000 + day * 100000 + seconds</c><br/>
		/// - <b>Date</b> — полный UTC день с Unix Epoch + секунда от начала дня<br/>
		///     Формула: <c>daysSinceEpoch * 100000 + seconds</c><br/>
		/// </summary>
		/// <example>
		/// Примеры:<br/>
		/// <b>Interval:</b> <c>3600_0</c> → каждые 1 час<br/>
		/// <b>Daily:</b> <c>36610_1</c> → 10:10:10<br/>
		/// <b>Weekly:</b> <c>1_600_2</c> → вторник 00:10:00<br/>
		/// <b>Monthly:</b> <c>15_36000_3</c> → 15 число, 10:00:00<br/>
		/// <b>Yearly:</b> <c>2_15_36000_4</c> → 16 марта, 10:00:00 (март = 2, т.к. с 0)<br/>
		/// <b>Date:</b> <c>19723_36000_5</c> → 15 мая 2024, 10:00:00 UTC
		/// </example>
		public sealed SchedulePointKind Kind => ScheduleUtility.GetKind(Code);

		/// <inheritdoc cref="Kind"/>
		public long Code { get; }
	}

	/// <summary>
	/// Определяет тит кодировки Schedule Point
	/// </summary>
	public enum SchedulePointKind
	{
		Interval,
		Daily,
		Weekly,
		Monthly,
		Yearly,
		Date
	}
}
