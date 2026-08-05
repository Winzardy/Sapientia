using System;
using System.Collections.Generic;
using System.Data;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia
{
	public static class ScheduleUtility
	{
		private const int WHILE_SAFEGUARD = 20;

		/// <inheritdoc cref="SchedulePointKind"/>
		public static SchedulePointKind GetKind(long raw)
		{
			var rawType = (int) Math.Abs(raw % ISchedulePoint.TYPE_OFFSET);
			return rawType.ToEnum<SchedulePointKind>();
		}

		public static SchedulePointDecode Decode<T>(this ref T point)
			where T : struct, ISchedulePoint
		{
			return point.Code;
		}

		/// <inheritdoc cref="GetKind(long)"/>
		public static SchedulePointKind GetKind<T>(this T point)
			where T : struct, ISchedulePoint
			=> GetKind(point.Code);

		public static bool IsPassed(this ScheduleScheme scheme, DateTime utcAt, DateTime utcNow)
			=> IsPassed(scheme.points, utcAt, utcNow);

		public static bool IsEmpty(this ScheduleScheme scheme)
			=> scheme.points.IsNullOrEmpty();

		public static int CalculatePassedPointCount(this in ScheduleScheme schedule, DateTime utcAt, DateTime utcNow)
		{
			return CalculatePassedPointCount(schedule.points, utcAt, utcNow);
		}

		public static int CalculatePassedPointCount<T>(this T[] points, DateTime utcAt, DateTime utcNow)
			where T : struct, ISchedulePoint
		{
			var count = 0;
			for (var i = 0; i < points.Length; i++)
			{
				count += CalculatePassedPointCount(ref points[i], utcAt, utcNow);
			}

			return count;
		}

		public static int CalculatePassedPointCount<T>(this ref T point, DateTime utcAt, DateTime utcNow)
			where T : struct, ISchedulePoint
		{
			var kind = point.GetKind();
			if (kind == SchedulePointKind.Interval)
			{
				var decode = point.Decode();
				if (decode.sec <= 0)
					return 0;
				var timeSpan = utcNow - utcAt;
				if (timeSpan.TotalSeconds <= 0)
					return 0;
				return (int) (timeSpan.TotalSeconds / decode.sec);
			}

			// Date — одноразовая точка: считается только если попала в окно (utcAt, utcNow)
			if (kind == SchedulePointKind.Date)
			{
				var date = ToDateTime(point.Code, utcAt);
				return utcAt < date && utcNow >= date ? 1 : 0;
			}

			var nextData = ToDateTime(ref point, utcAt);
			var count = 0;
			while (utcNow > nextData)
			{
				count++;
				var dateTime = ToDateTime(ref point, nextData);
				if (dateTime <= nextData)
					throw new InvalidOperationException(
						"Invalid SchedulePoint progression. " +
						$"Next DateTime [ {dateTime:O} ] is not greater than previous [ {nextData:O} ] " +
						$"(code: {point.Code}, utcAt: {utcAt:O}, utcNow: {utcNow:O})"
					);

				nextData = dateTime;
			}

			return count;
		}

		public static bool IsPassed<T>(this T[] points, DateTime utcAt, DateTime utcNow)
			where T : struct, ISchedulePoint
		{
			for (var i = 0; i < points.Length; i++)
			{
				if (IsPassed(ref points[i], utcAt, utcNow))
					return true;
			}

			return false;
		}

		public static bool IsPassed<T>(this IEnumerable<T> points, DateTime utcAt, DateTime utcNow)
			where T : struct, ISchedulePoint
		{
			foreach (var point in points)
			{
				if (IsPassed(point, utcAt, utcNow))
					return true;
			}

			return false;
		}

		public static bool IsPassed<T>(this ref T point, DateTime utcAt, DateTime utcNow)
			where T : struct, ISchedulePoint
			=> IsPassed(point.Code, utcAt, utcNow);

		public static bool IsPassed(this ISchedulePoint point, DateTime utcAt, DateTime utcNow)
			=> IsPassed(point.Code, utcAt, utcNow);

		public static bool IsPassed(long rawCode, DateTime utcAt, DateTime utcNow)
		{
			var dateTime = ToDateTime(rawCode, utcAt, out var kind);

			if (utcAt == utcNow)
				return utcAt >= dateTime;

			// Date — одноразовая точка: учитывается только если попала в окно (utcAt, utcNow)
			if (kind == SchedulePointKind.Date)
				return utcNow >= dateTime && utcAt < dateTime;

			return utcNow > dateTime && utcAt < dateTime;
		}

		#region Window

		/// <summary>Длительность окна точки по индексу: вне массива — момент</summary>
		public static long GetWindowDuration(this in ScheduleScheme scheme, int index)
		{
			var durations = scheme.durations;

			if (durations == null || index < 0 || index >= durations.Length)
				return 0;

			return Math.Max(0, durations[index]);
		}

		/// <inheritdoc cref="TryGetActiveWindow(ref ScheduleScheme, DateTime, DateTime, out DateTime, out DateTime)"/>
		public static bool IsActive(this ref ScheduleScheme scheme, DateTime utcAt, DateTime utcNow)
			=> TryGetActiveWindow(ref scheme, utcAt, utcNow, out _, out _);

		/// <summary>
		/// Окно схемы, накрывающее <paramref name="utcNow"/>. Если активных несколько,
		/// отдаётся то, что кончается позже — именно его показывает таймер «до конца»
		/// </summary>
		/// <param name="utcAt">
		/// Точка отсчёта расписания — как во всём остальном <see cref="ScheduleUtility"/>:
		/// вхождения до неё не существуют, Interval раскладывается от неё
		/// </param>
		public static bool TryGetActiveWindow(this ref ScheduleScheme scheme, DateTime utcAt, DateTime utcNow,
			out DateTime start, out DateTime end)
		{
			start = default;
			end = default;

			if (scheme.points.IsNullOrEmpty())
				return false;

			var found = false;

			for (var i = 0; i < scheme.points.Length; i++)
			{
				var duration = scheme.GetWindowDuration(i);

				if (duration <= 0)
					continue;

				if (!TryGetActiveWindow(ref scheme.points[i], duration, utcAt, utcNow, out var pointStart, out var pointEnd))
					continue;

				if (found && pointEnd <= end)
					continue;

				found = true;
				start = pointStart;
				end = pointEnd;
			}

			return found;
		}

		/// <inheritdoc cref="TryGetActiveWindow{T}(ref T, long, DateTime, DateTime, out DateTime, out DateTime)"/>
		public static bool IsActive<T>(this ref T point, long duration, DateTime utcAt, DateTime utcNow)
			where T : struct, ISchedulePoint
			=> TryGetActiveWindow(ref point, duration, utcAt, utcNow, out _, out _);

		/// <summary>Окно точки, накрывающее <paramref name="utcNow"/></summary>
		/// <param name="duration">
		/// Длительность окна в секундах — лежит рядом с точкой, в
		/// <see cref="ScheduleScheme.durations"/>
		/// </param>
		/// <param name="utcAt">Точка отсчёта расписания: вхождения до неё не существуют</param>
		/// <remarks>
		/// Хватает одного <see cref="ToDateTime{T}"/>: активным может быть только окно,
		/// начавшееся не раньше, чем <paramref name="duration"/> назад, а ближайшее вхождение
		/// от этой границы — единственный кандидат. Перебирать вхождения не нужно
		/// </remarks>
		public static bool TryGetActiveWindow<T>(this ref T point, long duration, DateTime utcAt, DateTime utcNow,
			out DateTime start, out DateTime end)
			where T : struct, ISchedulePoint
		{
			start = default;
			end = default;

			if (duration <= 0 || utcNow < utcAt)
				return false;

			// У Interval окна нет: он без абсолютной привязки, «отрезок каждые N секунд» —
			// это уже не расписание-момент, а другой механизм. Валидация в инспекторе не даст
			// задать ему длительность
			if (point.GetKind() == SchedulePointKind.Interval)
				return false;

			var from = SubtractSeconds(utcNow, duration);

			// Расписание ещё не началось — окна до якоря не существуют
			if (from < utcAt)
				from = utcAt;

			var candidate = ToDateTime(point.Code, from);

			// Верхняя граница — из-за Date: он отдаёт свою дату при любом курсоре, и без
			// проверки разовое окно осталось бы активным навсегда
			if (candidate <= from || candidate > utcNow)
				return false;

			start = candidate;
			end = candidate.AddSeconds(duration);

			return true;
		}

		/// <summary>
		/// Минимальный период повторения в секундах: окно длиннее него начнёт накладываться
		/// само на себя. У неповторяющихся видов — <see cref="long.MaxValue"/>
		/// </summary>
		public static long GetMinPeriodSeconds(SchedulePointKind kind)
			=> kind switch
			{
				SchedulePointKind.Daily => TimeUtility.SECS_IN_ONE_DAY,
				SchedulePointKind.Weekly => TimeUtility.SECS_IN_ONE_DAY * 7L,

				// Февраль — самый короткий месяц, по нему и считаем нижнюю границу
				SchedulePointKind.Monthly or SchedulePointKind.MonthlyOnWeekday
					=> TimeUtility.SECS_IN_ONE_DAY * 28L,

				SchedulePointKind.Yearly or SchedulePointKind.YearlyOnWeekday
					=> TimeUtility.SECS_IN_ONE_DAY * 365L,

				_ => long.MaxValue
			};

		private static DateTime SubtractSeconds(DateTime utc, long seconds)
		{
			var limit = (utc - DateTime.MinValue).TotalSeconds;
			return seconds >= limit ? DateTime.MinValue : utc.AddSeconds(-seconds);
		}

		#endregion

		/// <returns>Ближайшую дату</returns>
		public static DateTime ToDateTime(this ScheduleScheme scheme, DateTime utcAt)
		{
			var dateTime = DateTime.MinValue;
			for (var i = 0; i < scheme.points.Length; i++)
			{
				var pointDateTime = ToDateTime(ref scheme.points[i], utcAt);
				if (dateTime == DateTime.MinValue || pointDateTime < dateTime)
					dateTime = pointDateTime;
			}

			return dateTime;
		}

		/// <returns>Интервал до ближайшей даты</returns>
		public static TimeSpan GetRemaining(this ScheduleScheme scheme, DateTime utcAt, DateTime utcNow)
		{
			var scheduledDateTime = scheme.ToDateTime(utcAt);
			return scheduledDateTime - utcNow;
		}

		public static DateTime ToDateTime<T>(this ref T point, DateTime utcAt)
			where T : struct, ISchedulePoint
			=> ToDateTime(point.Code, utcAt);

		public static DateTime ToDateTime(this ISchedulePoint point, DateTime utcAt)
			=> ToDateTime(point.Code, utcAt);

		private static DateTime ToDateTime(long rawCode, DateTime utcAt)
		{
			return ToDateTime(rawCode, utcAt, out _);
		}

		private static DateTime ToDateTime(long rawCode, DateTime utcAt, out SchedulePointKind kind)
		{
			SchedulePointDecode decode = rawCode;
			kind = decode.kind;
			var decodeMonthToDateMonth = decode.mh + 1;
			var decodeDayToDateDay = decode.day + 1;
			switch (decode.kind)
			{
				case SchedulePointKind.Interval:
					return utcAt.AddSeconds(decode.sec);

				case SchedulePointKind.Date:
				{
					return new DateTime(
						(int) decode.yr,
						decodeMonthToDateMonth,
						(int) decodeDayToDateDay,
						decode.hr,
						decode.min,
						(int) decode.sec,
						DateTimeKind.Utc);
				}

				case SchedulePointKind.Daily:
				{
					var dailyStart = new DateTime(utcAt.Year, utcAt.Month, utcAt.Day,
						0, 0, 0, kind: DateTimeKind.Utc);
					var timeOfDay = new TimeSpan(decode.hr, decode.min, (int) decode.sec);
					if (utcAt.TimeOfDay >= timeOfDay)
						dailyStart = dailyStart.AddDays(1);

					return dailyStart.Add(timeOfDay);
				}

				case SchedulePointKind.Monthly:
				{
					var monthStart = new DateTime(utcAt.Year, utcAt.Month, 1,
						0, 0, 0, kind: DateTimeKind.Utc);

					var monthlyDate = GetMonthlyDate(in decode, monthStart);
					if (monthlyDate <= utcAt)
						monthlyDate = GetMonthlyDate(in decode, monthStart.AddMonths(1));

					return monthlyDate;
				}

				case SchedulePointKind.Yearly:
				{
					var yearlyDate = GetYearlyDate(in decode, utcAt.Year);
					if (yearlyDate <= utcAt)
						yearlyDate = GetYearlyDate(in decode, utcAt.Year + 1);

					return yearlyDate;
				}

				case SchedulePointKind.Weekly:
				{
					var daysSinceWeekStart = (utcAt.DayOfWeek.ToInt() + 6) % 7;
					var weeklyStart = new DateTime(utcAt.Year, utcAt.Month, utcAt.Day,
						0, 0, 0, DateTimeKind.Utc
					).AddDays(-daysSinceWeekStart);

					var weeklyDate = weeklyStart
						.AddDays(decode.day)
						.AddHours(decode.hr)
						.AddMinutes(decode.min)
						.AddSeconds(decode.sec);

					if (weeklyDate <= utcAt)
						weeklyDate = weeklyDate.AddDays(7);

					return weeklyDate;
				}

				case SchedulePointKind.MonthlyOnWeekday:
				{
					return GetMonthlyOnWeekdayDate(in decode, utcAt);
				}

				case SchedulePointKind.YearlyOnWeekday:
				{
					return GetYearlyOnWeekdayDate(in decode, utcAt);
				}

				default:
					return utcAt;
			}
		}

		private static DateTime GetMonthlyDate(in SchedulePointDecode decode, in DateTime monthStart)
		{
			var daysInMonth = DateTime.DaysInMonth(monthStart.Year, monthStart.Month);
			// День зажимается в границы месяца: «31-е число» в феврале = последний день февраля
			var day = decode.sign
				? Math.Min(decode.day + 1, daysInMonth)
				: Math.Max(daysInMonth - decode.day, 1);

			return monthStart
				.AddDays(day - 1)
				.AddHours(decode.hr)
				.AddMinutes(decode.min)
				.AddSeconds(decode.sec);
		}

		private static DateTime GetYearlyDate(in SchedulePointDecode decode, int year)
		{
			var monthStart = new DateTime(year, decode.mh + 1, 1,
				0, 0, 0, kind: DateTimeKind.Utc);
			return GetMonthlyDate(in decode, monthStart);
		}

		private static DateTime GetMonthlyOnWeekdayDate(in SchedulePointDecode decode, in DateTime utcAt)
		{
			var year = utcAt.Year;
			var month = utcAt.Month;
			var dayOfWeek = (int) decode.day + 1;
			int weekIndex = decode.weekOfMonth;

			var t = WHILE_SAFEGUARD;
			while (t-- > 0)
			{
				if (TryGetDateTime(in decode, out var date) && date > utcAt)
					return date;

				AddMonth();
			}

			return DateTime.MinValue;

			bool TryGetDateTime(in SchedulePointDecode decode, out DateTime dateTime)
			{
				dateTime = DateTime.MinValue;
				if (decode.sign)
				{
					var first = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
					var firstDay = (int) first.DayOfWeek;
					var offset = (dayOfWeek - firstDay + 7) % 7 + weekIndex * 7;

					var targetDay = offset + 1;
					if (targetDay > DateTime.DaysInMonth(year, month))
						return false;

					dateTime = new DateTime(year, month, targetDay, decode.hr, decode.min,
						(int) decode.sec, DateTimeKind.Utc);
					return true;
				}
				else
				{
					var daysInMonth = DateTime.DaysInMonth(year, month);
					var last = new DateTime(year, month, daysInMonth, 0, 0, 0, DateTimeKind.Utc);
					var lastDay = (int) last.DayOfWeek;
					var offset = (lastDay - dayOfWeek + 7) % 7 + weekIndex * 7;

					var targetDay = daysInMonth - offset;
					if (targetDay <= 0)
						return false;

					dateTime = new DateTime(year, month, targetDay, decode.hr, decode.min,
						(int) decode.sec, DateTimeKind.Utc);
					return true;
				}
			}

			void AddMonth()
			{
				if (++month <= 12)
					return;

				month = 1;
				year++;
			}
		}

		private static DateTime GetYearlyOnWeekdayDate(in SchedulePointDecode decode, in DateTime utcAt)
		{
			var year = utcAt.Year;
			var targetMonth = decode.mh + 1;
			var dayOfWeek = (int) decode.day + 1;
			int weekIndex = decode.weekOfMonth;

			var t = WHILE_SAFEGUARD;
			while (t-- > 0)
			{
				if (TryGetDateTime(in decode, out var dateTime) && dateTime > utcAt)
					return dateTime;

				year++;
			}

			return DateTime.MinValue;

			bool TryGetDateTime(in SchedulePointDecode decode, out DateTime date)
			{
				date = DateTime.MinValue;

				if (decode.sign)
				{
					var first = new DateTime(year, targetMonth, 1, 0, 0, 0, DateTimeKind.Utc);
					var firstDay = (int) first.DayOfWeek;
					var offset = (dayOfWeek - firstDay + 7) % 7 + weekIndex * 7;

					var targetDay = offset + 1;
					if (targetDay > DateTime.DaysInMonth(year, targetMonth))
						return false;

					date = new DateTime(year, targetMonth, targetDay, decode.hr, decode.min, (int) decode.sec, DateTimeKind.Utc);
				}
				else
				{
					var daysInMonth = DateTime.DaysInMonth(year, targetMonth);
					var last = new DateTime(year, targetMonth, daysInMonth, 0, 0, 0, DateTimeKind.Utc);
					var lastDay = (int) last.DayOfWeek;
					var offset = (lastDay - dayOfWeek + 7) % 7 + weekIndex * 7;

					var targetDay = daysInMonth - offset;
					if (targetDay <= 0)
						return false;

					date = new DateTime(year, targetMonth, targetDay, decode.hr, decode.min, (int) decode.sec, DateTimeKind.Utc);
				}

				return true;
			}
		}
	}
}
