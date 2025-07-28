using System;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class TimeUtility
	{
		public const int SECS_IN_ONE_DAY = 86400;
		public const int SECS_IN_ONE_HOUR = 3600;
		public const int SECS_IN_ONE_MINUTE = 60;

		public const int MINS_IN_ONE_HOUR = SECS_IN_ONE_MINUTE;

		public const string ALREADY_PASSED = "already passed";

		public const string MILLISECOND_LABEL = "ms";
		public const string SECOND_LABEL = "sec";
		public const string MINUTE_LABEL = "min";
		public const string HOUR_LABEL = "hr";
		public const string DAY_LABEL = "day";
		public const string SHORT_DAY_LABEL = "d";
		public const string MONTH_LABEL = "mh";
		public const string WEEK_LABEL = "wk";
		public const string YEAR_LABEL = "yr";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ToSec(this int ms) => ms / 1000f;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToMS(this float seconds) => (int) (seconds * 1000);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long ToTicks(this int ms) => TimeSpan.TicksPerMillisecond * ms;

		/// <summary>
		/// {days} d {hours} h {minutes} m {seconds} s
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel(this float seconds, bool useSpace = true)
		{
			if (seconds < 1)
				seconds = 1;

			var timeSpan = new TimeSpan(0, 0, 0, (int) seconds);
			return ToLabel(timeSpan, useSpace, false);
		}

		/// <summary>
		/// {days} d {hours} h {minutes} m {seconds} s
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabelFromInt(this int secs, bool useSpace = true, bool useMs = false)
		{
			if (secs < 1)
				secs = 1;

			return ToLabel(TimeSpan.FromSeconds(secs), useSpace, useMs);
		}

		/// <summary>
		/// {days} d {hours} h {minutes} m {seconds} s
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabelFromLong(this long secs, bool useSpace = true, bool useMs = false)
		{
			if (secs < 1)
				secs = 1;

			return ToLabel(TimeSpan.FromSeconds(secs), useSpace, useMs);
		}


		/// <summary>
		/// {days} d {hours} h {minutes} m {seconds} s
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel(this int ms, bool useSpace = true)
		{
			var timeSpan = new TimeSpan(0, 0, 0, 0, ms);
			return ToLabel(timeSpan, useSpace);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel(this TimeSpan ts, bool useSpace = true, bool useMs = true)
		{
			if (ts.TotalSeconds <= 0)
				return ALREADY_PASSED;

			var str = string.Empty;

			var space = useSpace ? " " : string.Empty;

			if (useMs && ts.Milliseconds > 0)
				str = $"{ts.Milliseconds}{space}{MILLISECOND_LABEL}";
			if (ts.Seconds > 0)
				str = $"{ts.Seconds}{space}{SECOND_LABEL} " + str;
			if (ts.TotalSeconds >= 60)
				str = $"{ts.Minutes}{space}{MINUTE_LABEL} " + str;
			if (ts.TotalSeconds >= 3600)
				str = $"{ts.Hours}{space}{HOUR_LABEL} " + str;
			if (ts.TotalSeconds >= 86400)
				str = $"{ts.Days}{space}{SHORT_DAY_LABEL} " + str;

			return str.Trim();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToDaySeconds(this DateTime dt)
		{
			return dt.Hour * 60 * 60 + dt.Minute * 60 + dt.Second;
		}

		public static DateTime ToDateTime(this long ticks) => new (ticks, DateTimeKind.Utc);
	}
}
