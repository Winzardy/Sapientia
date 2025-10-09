using System;
using Sapientia.Extensions;

namespace Sapientia
{
	public enum CanApplyUsageErrorCode
	{
		None,

		/// <summary>
		/// Превышено количество использований и нет кулдауна
		/// </summary>
		PermanentlyExhausted,

		/// <summary>
		/// Превышено количество использований, но есть кулдаун
		/// </summary>
		TemporarilyExhausted,

		Cooldown
	}

	public static class UsageLimitUtility
	{
		public static bool IsEmpty(this UsageLimitScheme scheme) => scheme.usageCount == 0;

		public static bool CanApplyUsage(this in UsageLimitScheme scheme, in UsageLimitState state, DateTime now,
			out UsageLimitApplyError? errorCode)
		{
			errorCode = null;

			// Без ограничений
			if (scheme.usageCount == 0)
				return true;

			// Первое использование
			if (state.usageCount == 0)
				return true;

			var firstUsageDateTime = state.firstUsageTimestamp.ToDateTime();
			if (scheme.IsResetState(firstUsageDateTime, now))
				return true;

			if (state.usageCount >= scheme.usageCount)
			{
				var remainingTime = scheme.GetRemainingTime(now, firstUsageDateTime);
				errorCode = new UsageLimitApplyError(in scheme,
					remainingTime > TimeSpan.Zero
						? CanApplyUsageErrorCode.TemporarilyExhausted
						: CanApplyUsageErrorCode.PermanentlyExhausted)
				{
					remainingTime = remainingTime
				};
				return false;
			}

			var lastUsageDateTime = state.lastUsageTimestamp.ToDateTime();
			if (!scheme.reset.IsEmpty() && !scheme.reset.IsPassed(lastUsageDateTime, now))
			{
				errorCode = new UsageLimitApplyError(in scheme, CanApplyUsageErrorCode.Cooldown)
				{
					remainingTime = scheme.GetRemainingTime(now, firstUsageDateTime, lastUsageDateTime)
				};
				return false;
			}

			return true;
		}

		public static bool IsResetState(this in UsageLimitScheme scheme, in UsageLimitState state, DateTime now)
			=> IsResetState(scheme, state.firstUsageTimestamp.ToDateTime(), now);

		public static bool IsResetState(this in UsageLimitScheme scheme, DateTime at, DateTime now)
			=> !scheme.fullReset.IsEmpty() && scheme.fullReset.IsPassed(at, now);

		public static int GetRemainingUsages(this in UsageLimitScheme scheme, in UsageLimitState state)
		{
			if (scheme.usageCount == 0)
				return UsageLimitScheme.INFINITY_USAGES;

			var remaining = scheme.usageCount - state.usageCount;
			return remaining.Max(0);
		}

		/// <returns>Если возвращает <c>null</c>, то нечего ждать</returns>
		public static TimeSpan? GetRemainingTime(this in UsageLimitScheme scheme, DateTime utcNow, DateTime firstUsageUtcAt,
			DateTime? utcAt = null)
		{
			var fullResetDateTime = scheme.fullReset.ToDateTime(firstUsageUtcAt);

			if (utcAt.HasValue)
			{
				var usageResetDateTime = scheme.reset.ToDateTime(utcAt.Value);

				if (fullResetDateTime > usageResetDateTime)
					return usageResetDateTime - utcNow;
			}

			var resetDateTime = fullResetDateTime - utcNow;

			if (resetDateTime <= TimeSpan.Zero)
				return null;

			return resetDateTime;
		}

		public static void ApplyUsage(this ref UsageLimitState state, in UsageLimitScheme scheme, DateTime now)
		{
			if (!CanApplyUsage(in scheme, in state, now, out var error))
				throw new InvalidOperationException($"Cannot apply usage by error: {error}");

			if (scheme.IsResetState(in state, now))
				ForceReset(ref state);

			state.usageCount++;
			state.lastUsageTimestamp = now.Ticks;

			if (state.usageCount == 1)
				state.firstUsageTimestamp = now.Ticks;
			if (state.usageCount >= scheme.usageCount)
				state.fullUsageCount++;
		}

		public static bool TryApplyUsage(this ref UsageLimitState state, in UsageLimitScheme scheme, DateTime now,
			out UsageLimitApplyError? error)
		{
			error = null;
			if (!CanApplyUsage(in scheme, in state, now, out error))
				return false;

			if (scheme.IsResetState(in state, now))
				ForceReset(ref state);

			state.usageCount++;
			state.lastUsageTimestamp = now.Ticks;

			if (state.usageCount == 1)
				state.firstUsageTimestamp = now.Ticks;
			if (state.usageCount >= scheme.usageCount)
				state.fullUsageCount++;

			return true;
		}

		/// <param name="now">Если передать текущее время, то он будет использоваться как последнее</param>
		public static void ForceReset(this ref UsageLimitState state, DateTime? now = null)
		{
			state.usageCount = 0;
			if (now.HasValue)
				state.lastUsageTimestamp = now.Value.Ticks;
		}

		/// <param name="now">Если передать текущее время, то он будет использоваться как последнее</param>
		public static void ForceApplyUsage(this ref UsageLimitState state, DateTime? now = null)
		{
			state.usageCount++;

			if (now.HasValue)
				state.lastUsageTimestamp = now.Value.Ticks;
		}
	}

	public struct UsageLimitApplyError
	{
		public UsageLimitScheme scheme;

		public CanApplyUsageErrorCode code;
		public TimeSpan? remainingTime;

		public UsageLimitApplyError(in UsageLimitScheme scheme, in CanApplyUsageErrorCode code) : this()
		{
			this.scheme = scheme;
			this.code = code;
		}
	}
}
