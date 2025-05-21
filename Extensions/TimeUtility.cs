using System;
using System.Runtime.CompilerServices;

namespace Sapientia.Extensions
{
	public static class TimeUtility
	{
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
		public static string ToLabel(this int ms, bool useSpace = true)
		{
			var timeSpan = new TimeSpan(0, 0, 0, 0, ms);
			return ToLabel(timeSpan, useSpace);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel(this TimeSpan timeSpan, bool useSpace = true, bool useMs = true)
		{
			var str = string.Empty;

			var space = useSpace ? " " : string.Empty;

			if (timeSpan.Milliseconds > 0)
				str = $"{timeSpan.Milliseconds}{space}ms";
			if (timeSpan.Seconds > 0)
				str = $"{timeSpan.Seconds}{space}s " + str;
			if (timeSpan.TotalSeconds >= 60)
				str = $"{timeSpan.Minutes}{space}m " + str;
			if (timeSpan.TotalSeconds >= 3600)
				str = $"{timeSpan.Hours}{space}h " + str;
			if (timeSpan.TotalSeconds >= 86400)
				str = $"{timeSpan.Days}{space}d " + str;

			return str.Trim();
		}
	}
}
