using System;
using System.Runtime.CompilerServices;
using Sapientia.Pooling;

namespace Sapientia.Extensions
{
	public static class TimeUtility
	{
		private const string SPACE = " ";
		private const string SEPARATOR = " ";

		public const int MS_IN_ONE_SECOND = 1000;

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
		public static float ToSec(this int ms) => ms / (float) MS_IN_ONE_SECOND;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToMilliseconds(this float seconds) => (int) (seconds * MS_IN_ONE_SECOND);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToMilliseconds(this int seconds) => seconds * MS_IN_ONE_SECOND;

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
		public static string ToLabel(this int seconds, bool useSpace = true, bool useMs = false)
		{
			if (seconds < 1)
				seconds = 1;

			return ToLabel(TimeSpan.FromSeconds(seconds), useSpace, useMs);
		}

		/// <summary>
		/// {days} d {hours} h {minutes} m {seconds} s
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel(this long seconds, bool useSpace = true, bool useMs = false)
		{
			if (seconds < 1)
				seconds = 1;

			return ToLabel(TimeSpan.FromSeconds(seconds), useSpace, useMs);
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

		public static string ToLabel(this TimeSpan ts, bool useSpace = true, bool useMs = true)
		{
			if (ts.TotalSeconds <= 0)
				return ALREADY_PASSED;

			using (StringBuilderPool.Get(out var sb))
			{
				var space = useSpace ? SPACE : string.Empty;

				if (useMs && ts.Milliseconds > 0)
					sb.Append($"{ts.Milliseconds}{space}{MILLISECOND_LABEL}{SEPARATOR}");
				else if (ts.Milliseconds > 0)
					ts = ts.Add(TimeSpan.FromSeconds(1));

				if (ts.TotalSeconds > 0)
					sb.Prepend($"{ts.Seconds}{space}{SECOND_LABEL}{SEPARATOR}");
				if (ts.TotalSeconds >= SECS_IN_ONE_MINUTE)
					sb.Prepend($"{ts.Minutes}{space}{MINUTE_LABEL}{SEPARATOR}");
				if (ts.TotalSeconds >= SECS_IN_ONE_HOUR)
					sb.Prepend($"{ts.Hours}{space}{HOUR_LABEL}{SEPARATOR}");
				if (ts.TotalSeconds >= SECS_IN_ONE_DAY)
					sb.Prepend($"{ts.Days}{space}{DAY_LABEL}{SEPARATOR}");

				return sb.ToString()
					.Trim();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int ToDaySeconds(this DateTime dt)
		{
			return dt.Hour * 60 * 60 + dt.Minute * 60 + dt.Second;
		}

		public static DateTime ToDateTime(this long ticks) => new(ticks, DateTimeKind.Utc);

		public static DateTime? ToDateTimeOrNull(this long timestamp) => timestamp > 0
			? new DateTime(timestamp, DateTimeKind.Utc)
			: null;
	}
}
