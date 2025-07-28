namespace Sapientia
{
	public interface ISchedulePoint
	{
		public const long TYPE_OFFSET = 10L;

		/// <summary>
		/// Закодированное время, декодирование зависит от типа:<br/>
		/// - <b><see cref="SchedulePointKind.Interval"/>:</b> — интервал в секундах с момента запуска<br/>
		///     <code>Код: (секунда)_(тип)</code><br/>
		/// - <b><see cref="SchedulePointKind.Date"/></b> — полный UTC день с Unix Epoch + секунда от начала дня<br/>
		///     <code>Код: (знак)(день от epoch)_(секунда от начала дня)_(тип)</code><br/>
		///
		/// - <b><see cref="SchedulePointKind.Daily"/></b> — секунда от начала дня (0–86399)<br/>
		///     <code>Код: (секунда от начала дня)_(тип)</code><br/>
		/// - <b><see cref="SchedulePointKind.Monthly"/></b> — день месяца и секунда от начала дня<br/>
		///     <code>Код: (знак)(номер дня)_(секунда от начала дня)_(тип)</code><br/>
		/// - <b><see cref="SchedulePointKind.Yearly"/></b> — месяц, день месяца и секунда от начала дня<br/>
		///     <code>Код: (месяц)_(день)_(секунда от начала дня)_(тип)</code><br/>
		///
		/// - <b><see cref="SchedulePointKind.Weekly"/></b> — день недели и секунда от начала дня<br/>
		///     <code>Код: (знак)(день недели)_(секунда от начала дня)_(тип)</code><br/>
		/// - <b><see cref="SchedulePointKind.MonthlyOnWeekday"/></b> — порядковый номер недели в месяце, день недели и секунда от начала дня<br/>
		///     <code>Код: (знак)(номер недели)_(день недели)_(секунда от начала дня)_(тип)</code><br/>
		/// - <b><see cref="SchedulePointKind.YearlyOnWeekday"/></b> — месяц, порядковый номер недели в месяце, день недели и секунда от начала дня<br/>
		///     <code>Код: (знак)(месяц)_(номер недели)_(день недели)_(секунда от начала дня)_(тип)</code>
		/// </summary>
		/// <example>
		/// Примеры:<br/>
		/// <b><see cref="SchedulePointKind.Interval"/>: </b> <c>3600_0</c> → каждые 1 час<br/>
		/// <b><see cref="SchedulePointKind.Date"/>: </b> <c>19723_36000_5</c> → 15 мая 2024, 10:00:00 UTC<br/>
		/// <b><see cref="SchedulePointKind.Daily"/>: </b> <c>36610_1</c> → 10:10:10<br/>
		/// <b><see cref="SchedulePointKind.Monthly"/>: </b> <c>15_36000_3</c> → 15 число, 10:00:00<br/>
		/// <b><see cref="SchedulePointKind.Yearly"/>: </b> <c>2_15_36000_4</c> → 16 марта, 10:00:00 (март = 2, т.к. с 0)<br/>
		/// <b><see cref="SchedulePointKind.Weekly"/>: </b> <c>1_600_2</c> → вторник 00:10:00<br/>
		/// <b><see cref="SchedulePointKind.MonthlyOnWeekday"/>: </b> <c>1_1_600_2</c> → вторая неделя месяца, вторник 00:10:00<br/>
		/// <b><see cref="SchedulePointKind.YearlyOnWeekday"/>: </b> <c>1_0_1_600_2</c> → февраль, первая неделя, вторник 00:10:00<br/>
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

		Date,

		Daily,
		Monthly,
		Yearly,

		Weekly, // День недели
		MonthlyOnWeekday, // Недельный день месяца
		YearlyOnWeekday, // Недельный день года
	}
}
