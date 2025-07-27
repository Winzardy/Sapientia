using System;
using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Sapientia
{
	public static class ScheduleUtility
	{
		private const int KIND_LENGHT = (int) SchedulePointKind.Date;

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

		public static bool IsPassed(this ResetSchedule schedule, DateTime utcAt, DateTime utcNow)
			=> IsPassed(schedule.points, utcAt, utcNow);

		public static bool IsEmpty(this ResetSchedule schedule)
			=> schedule.points.IsNullOrEmpty();

		public static bool IsPassed<T>(this T[] points, DateTime utcAt, DateTime utcNow)
			where T : struct, ISchedulePoint
		{
			for (int i = 0; i < points.Length; i++)
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
			=> utcNow > GetNextDateTime(rawCode, utcAt);

		public static DateTime GetNextDateTime<T>(this ref T point, DateTime utcAt)
			where T : struct, ISchedulePoint
			=> GetNextDateTime(point.Code, utcAt);

		public static DateTime GetNextDateTime(this ISchedulePoint point, DateTime utcAt)
			=> GetNextDateTime(point.Code, utcAt);

		private static DateTime GetNextDateTime(long rawCode, DateTime utcAt)
		{
			SchedulePointDecode decode = rawCode;
			switch (decode.kind)
			{
				case SchedulePointKind.Interval:
					return utcAt.AddSeconds(decode.sec);

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

				case SchedulePointKind.Weekly:
				{
					var daysSinceWeekStart = (utcAt.DayOfWeek.ToInt() + 6) % 7;
					var weeklyStart = new DateTime(
						utcAt.Year, utcAt.Month, utcAt.Day, 0, 0, 0, DateTimeKind.Utc
					).AddDays(-daysSinceWeekStart);

					var targetTime = weeklyStart
					   .AddDays(decode.day)
					   .AddHours(decode.hr)
					   .AddMinutes(decode.min)
					   .AddSeconds(decode.sec);

					if (targetTime <= utcAt)
						targetTime = targetTime.AddDays(7);

					return targetTime;
				}

				case SchedulePointKind.Monthly:
				{
					var monthlyStart = new DateTime(utcAt.Year, utcAt.Month, 1,
						0, 0, 0, kind: DateTimeKind.Utc);

					if (utcAt.Day >= decode.day)
						monthlyStart = monthlyStart.AddMonths(1);

					if (!decode.sign)
						monthlyStart = monthlyStart
						   .AddMonths(1)
						   .AddDays(-1);
					;

					var sign = decode.sign ? 1 : -1;
					return monthlyStart
					   .AddDays(sign * decode.day)
					   .AddHours(decode.hr)
					   .AddMinutes(decode.min)
					   .AddSeconds(decode.sec);
				}

				case SchedulePointKind.Yearly:
				{
					var yearlyStart = new DateTime(utcAt.Year, 1, 1,
						0, 0, 0, kind: DateTimeKind.Utc);
					if (utcAt.Month >= decode.mh)
						yearlyStart = yearlyStart.AddYears(1);

					if (!decode.sign)
						yearlyStart = yearlyStart
						   .AddMonths(1)
						   .AddDays(-1);

					var sign = decode.sign ? 1 : -1;
					return yearlyStart
					   .AddMonths(decode.mh)
					   .AddDays(sign * decode.day)
					   .AddHours(decode.hr)
					   .AddMinutes(decode.min)
					   .AddSeconds(decode.sec);
				}

				case SchedulePointKind.Date:
				{
					return new DateTime(
						(int) decode.yr,
						(int) decode.mh + 1,
						(int) decode.day + 1,
						decode.hr,
						decode.min,
						(int) decode.sec,
						DateTimeKind.Utc);
				}

				default:
					return utcAt;
			}
		}
	}
}
