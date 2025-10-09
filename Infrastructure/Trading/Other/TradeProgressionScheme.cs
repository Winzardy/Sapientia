using System;
using Sapientia;
#if CLIENT
using UnityEngine;
#endif

namespace Trading
{
	// Выглядит так что это можно использовать не только в Trading
	[Serializable]
	public struct TradeProgressionScheme
	{
		/// <summary>
		/// Тип авто сброса: <br/>
		/// <b>None</b> - отсутствует <br/>
		/// <b>Full</b> - полный сброс <br/>
		/// </summary>
		public TradeProgressionResetType type;

		/// <summary>
		/// Расписание при котором будет происходить сброс
		/// </summary>
		public ScheduleScheme schedule;

		/// <summary>
		/// Если <c>true</c>, то расписание будет опираться на настоящие время (без учета виртуального времени)
		/// </summary>
		public bool realTime;
	}

	public enum TradeProgressionResetType
	{
		None,

#if CLIENT
		[Tooltip("Полная очистка")]
		Full,
#endif
		// Incremental
	}

	[Serializable]
	public struct TradeProgressionState
	{
		public int current;
		public int total;

		/// <see cref="DateTime.Ticks"/>
		public long firstIncrementTimestamp;

		public long lastIncrementTimestamp;
	}

	public static class TradeProgressionUtility
	{
		public static int GetCurrentProgress(this in TradeProgressionState state, in TradeProgressionScheme scheme,
			DateTime now, DateTime? nowWithoutOffset = null)
		{
			if (scheme.type == TradeProgressionResetType.None)
				return state.current;

			var utcAt = new DateTime(state.firstIncrementTimestamp);
			var dateTime = scheme.realTime
				? nowWithoutOffset
				?? now
				: now;

			switch (scheme.type)
			{
				case TradeProgressionResetType.Full:

					if (scheme.schedule.IsPassed(utcAt, dateTime))
						return 0;

					return state.current;

				// case TradeProgressionResetType.Incremental:
				// 	var passedPointCount = scheme.schedule.CalculatePassedPointCount(utcAt, dateTime);
				// 	return state.current - passedPointCount;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void IncrementProgress(this ref TradeProgressionState state, in TradeProgressionScheme scheme,
			DateTime now, DateTime? nowWithoutOffset = null)
		{
			var dateTime = scheme.realTime
				? nowWithoutOffset
				?? now
				: now;

			DecrementProgress(ref state, in scheme, dateTime);

			state.current++;
			state.total++;
			if (state.current == 1)
				state.firstIncrementTimestamp = dateTime.Ticks;
			state.lastIncrementTimestamp = dateTime.Ticks;
		}

		private static void DecrementProgress(this ref TradeProgressionState state, in TradeProgressionScheme scheme, DateTime now)
		{
			var utcAt = new DateTime(state.firstIncrementTimestamp);

			switch (scheme.type)
			{
				case TradeProgressionResetType.Full:
					if (scheme.schedule.IsPassed(utcAt, now))
						ResetProgress(ref state, in scheme, now);

					break;

				// case TradeProgressionResetType.Incremental:
				//
				// 	break;
			}
		}

		public static void ResetProgress(this ref TradeProgressionState state, in TradeProgressionScheme scheme, DateTime now,
			DateTime? nowWithoutOffset = null)
		{
			var dateTime = scheme.realTime
				? nowWithoutOffset
				?? now
				: now;

			ResetProgress(ref state, in scheme, dateTime);
		}

		public static void ResetProgress(this ref TradeProgressionState state, in TradeProgressionScheme scheme, DateTime now)
		{
			state.current = 0;
			state.firstIncrementTimestamp = 0;
		}
	}
}
