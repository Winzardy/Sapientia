using System;
using Sapientia;
using Sapientia.Conditions;
using UnityEngine;

namespace Trading
{
	public enum TradeProgressionSchemeSource
	{
		[Tooltip("Свой личный прогресс")]
		Local,

		[Tooltip("Общий прогресс между несколькими трейд участниками")]
		Shared
	}

	// Выглядит так что это можно использовать не только в Trading
	[Serializable]
	public class TradeProgressionScheme
	{
		public const string BOARD_PROGRESS_KEY_FORMAT = "TradeProgress_{0}";
		public const string PROGRESS_GUID_KEY_FORMAT = "{0}_{1}";

		/// <summary>
		/// Условие при котором награда прогрессирует, если 'None', то прогрессирует всегда
		/// </summary>
		[SerializeReference]
		public Condition<Blackboard> condition = new ObjectProviderBlackboardProxyEvaluator();

		/// <summary>
		/// Тип запланированного сброса: <br/>
		/// <b>None</b> - отсутствует <br/>
		/// <b>Full</b> - полный сброс <br/>
		/// </summary>
		public TradeProgressionScheduleResetType type;

		/// <summary>
		/// Расписание при котором будет происходить сброс
		/// </summary>
		public ScheduleScheme schedule;
	}

	public enum TradeProgressionScheduleResetType
	{
		None,

		[Tooltip("Полная очистка")]
		Full,

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
			DateTime dateTime)
		{
			if (scheme.type == TradeProgressionScheduleResetType.None)
				return state.current;

			var utcAt = new DateTime(state.firstIncrementTimestamp);

			switch (scheme.type)
			{
				case TradeProgressionScheduleResetType.Full:

					if (scheme.schedule.IsPassed(utcAt, dateTime))
						return 0;

					return state.current;

				// case TradeProgressionResetType.Incremental:
				// var passedPointCount = scheme.schedule.CalculatePassedPointCount(utcAt, dateTime);
				// return state.current - passedPointCount;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public static void IncrementProgress(this ref TradeProgressionState state, in TradeProgressionScheme scheme,
			DateTime dateTime)
		{
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
				case TradeProgressionScheduleResetType.Full:
					if (scheme.schedule.IsPassed(utcAt, now))
						ResetProgress(ref state, in scheme, now);

					break;

				// case TradeProgressionResetType.Incremental:
				//
				// 	break;
			}
		}

		public static void ResetProgress(this ref TradeProgressionState state, in TradeProgressionScheme scheme, DateTime dateTime)
		{
			state.current                 = 0;
			state.firstIncrementTimestamp = 0;
		}
	}
}
