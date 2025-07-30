using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia
{
	public static class ScheduleUtility
	{
		private static readonly int KIND_LENGHT = Enum.GetValues(typeof(SchedulePointKind)).Length;
		private const int WHILE_SAFEGUARD = 20;

		/// <inheritdoc cref="SchedulePointKind"/>
		public static SchedulePointKind GetKind(long raw)
		{
			var rawType = (int) Math.Clamp(Math.Abs(raw % ISchedulePoint.TYPE_OFFSET), 0, KIND_LENGHT);
			return rawType.ToEnum<SchedulePointKind>();
		}

		/// <inheritdoc cref="GetKind(long)"/>
		public static SchedulePointKind GetKind<T>(this T point)
			where T : struct, ISchedulePoint
			=> GetKind(point.Code);

		public static bool IsPassed(this ScheduleEntry schedule, DateTime utcAt, DateTime utcNow)
			=> IsPassed(schedule.points, utcAt, utcNow);

		public static bool IsEmpty(this ScheduleEntry schedule)
			=> schedule.points.IsNullOrEmpty();

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
			=> utcNow > ToDateTime(rawCode, utcAt);

		/// <returns>Ближайшую дату</returns>
		public static DateTime ToDateTime(this ScheduleEntry entry, DateTime utcAt)
		{
			var dateTime = DateTime.MinValue;
			for (var i = 0; i < entry.points.Length; i++)
			{
				var pointDateTime = ToDateTime(ref entry.points[i], utcAt);
				if (dateTime == DateTime.MinValue || pointDateTime < dateTime)
					dateTime = pointDateTime;
			}

			return dateTime;
		}

		public static DateTime ToDateTime<T>(this ref T point, DateTime utcAt)
			where T : struct, ISchedulePoint
			=> ToDateTime(point.Code, utcAt);

		public static DateTime ToDateTime(this ISchedulePoint point, DateTime utcAt)
			=> ToDateTime(point.Code, utcAt);

		private static DateTime ToDateTime(long rawCode, DateTime utcAt)
		{
			SchedulePointDecode decode = rawCode;
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
					if (utcAt.Hour >= decode.hr)
						dailyStart = dailyStart.AddDays(1);

					return dailyStart
					   .AddHours(decode.hr)
					   .AddMinutes(decode.min)
					   .AddSeconds(decode.sec);
				}

				case SchedulePointKind.Monthly:
				{
					var monthlyStart = new DateTime(utcAt.Year, utcAt.Month, 1,
						0, 0, 0, kind: DateTimeKind.Utc);

					var sign = decode.sign ? 1 : -1;
					if (!decode.sign)
						monthlyStart = monthlyStart
						   .AddMonths(1)
						   .AddDays(-1);

					var monthlyDate = monthlyStart
					   .AddDays(sign * decode.day)
					   .AddHours(decode.hr)
					   .AddMinutes(decode.min)
					   .AddSeconds(decode.sec);

					if (monthlyDate < utcAt)
						monthlyDate = monthlyDate.AddYears(1);

					return monthlyDate;
				}

				case SchedulePointKind.Yearly:
				{
					var yearlyDate = new DateTime(utcAt.Year, decodeMonthToDateMonth, 1,
						0, 0, 0, kind: DateTimeKind.Utc);

					var sign = decode.sign ? 1 : -1;

					if (!decode.sign)
						yearlyDate = yearlyDate
						   .AddMonths(1)
						   .AddDays(-1);

					yearlyDate = yearlyDate.AddDays(sign * decode.day)
					   .AddHours(decode.hr)
					   .AddMinutes(decode.min)
					   .AddSeconds(decode.sec);

					if (yearlyDate < utcAt)
						yearlyDate = yearlyDate.AddYears(1);

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

		private static DateTime GetMonthlyOnWeekdayDate(in SchedulePointDecode decode, in DateTime utcAt)
		{
			var year = utcAt.Year;
			var month = utcAt.Month;
			var dayOfWeek = (int) decode.day + 1;
			int weekIndex = decode.weekOfMonth;

			DateTime date;
			var t = WHILE_SAFEGUARD;
			while (!TryGetDateTime(in decode, out date))
			{
				if (t-- <= 0)
					return DateTime.MinValue;

				AddMonth();
			}

			if (date <= utcAt)
			{
				AddMonth();

				if (TryGetDateTime(in decode, out date))
					return date;
			}

			return date;

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

			DateTime dateTime;

			var t = WHILE_SAFEGUARD;
			while (!TryGetDateTime(in decode, out dateTime))
			{
				if (t-- <= 0)
					return DateTime.MinValue;

				year++;
			}

			if (dateTime <= utcAt)
			{
				year++;
				if (TryGetDateTime(in decode, out dateTime))
					return dateTime;
			}

			return dateTime;

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
					return true;
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
					return true;
				}
			}
		}
	}
}
