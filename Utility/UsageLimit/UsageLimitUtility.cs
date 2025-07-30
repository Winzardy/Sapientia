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
		public static bool IsEmpty(this UsageLimitEntry entry) => entry.usageCount == 0;

		public static bool CanApplyUsage(in UsageLimitEntry entry, in UsageLimitModel model, DateTime now,
			out UsageLimitApplyError? errorCode)
		{
			errorCode = null;

			// Без ограничений
			if (entry.usageCount == 0)
				return true;

			// Первое использование
			if (model.usageCount == 0)
				return true;

			var firstUsageDateTime = model.firstUsageDate.ToDateTime();
			if (entry.IsResetState(firstUsageDateTime, now))
				return true;

			if (model.usageCount >= entry.usageCount)
			{
				var remainingTime = entry.GetRemainingTime(now, firstUsageDateTime);
				errorCode = new UsageLimitApplyError(in entry,
					remainingTime > TimeSpan.Zero
						? CanApplyUsageErrorCode.TemporarilyExhausted
						: CanApplyUsageErrorCode.PermanentlyExhausted)
				{
					remainingTime = remainingTime
				};
				return false;
			}

			var lastUsageDateTime = model.lastUsageDate.ToDateTime();
			if (!entry.reset.IsEmpty() && !entry.reset.IsPassed(lastUsageDateTime, now))
			{
				errorCode = new UsageLimitApplyError(in entry, CanApplyUsageErrorCode.Cooldown)
				{
					remainingTime = entry.GetRemainingTime(now, firstUsageDateTime, lastUsageDateTime)
				};
				return false;
			}

			return true;
		}

		public static bool IsResetState(this in UsageLimitEntry entry, in UsageLimitModel model, DateTime now)
			=> IsResetState(entry, model.firstUsageDate.ToDateTime(), now);

		public static bool IsResetState(this in UsageLimitEntry entry, DateTime at, DateTime now)
			=> !entry.fullReset.IsEmpty() && entry.fullReset.IsPassed(at, now);

		public static int GetRemainingUsages(this in UsageLimitEntry entry, in UsageLimitModel model)
		{
			if (entry.usageCount == 0)
				return UsageLimitEntry.INFINITY_USAGES;

			var remaining = entry.usageCount - model.usageCount;
			return remaining.Max(0);
		}

		/// <returns>Если возвращает <c>null</c>, то нечего ждать</returns>
		public static TimeSpan? GetRemainingTime(this in UsageLimitEntry entry, DateTime utcNow, DateTime firstUsageUtcAt,
			DateTime? utcAt = null)
		{
			var fullResetDateTime = entry.fullReset.ToDateTime(firstUsageUtcAt);

			if (utcAt.HasValue)
			{
				var usageResetDateTime = entry.reset.ToDateTime(utcAt.Value);

				if (fullResetDateTime > usageResetDateTime)
					return usageResetDateTime - utcNow;
			}

			var resetDateTime = fullResetDateTime - utcNow;

			if (resetDateTime <= TimeSpan.Zero)
				return null;

			return resetDateTime;
		}

		public static void ApplyUsage(ref UsageLimitModel model, in UsageLimitEntry entry, DateTime now)
		{
			if (!CanApplyUsage(in entry, in model, now, out var errorCode))
				throw new InvalidOperationException($"Cannot apply usage by error code [ {errorCode} ]");

			if (entry.IsResetState(in model, now))
				ForceReset(ref model);

			model.usageCount++;
			model.lastUsageDate = now.Ticks;

			if (model.usageCount == 1)
				model.firstUsageDate = now.Ticks;
		}

		/// <param name="now">Если передать текущее время, то он будет использоваться как последнее</param>
		public static void ForceReset(ref UsageLimitModel model, DateTime? now = null)
		{
			model.usageCount = 0;
			if (now.HasValue)
				model.lastUsageDate = now.Value.Ticks;
		}

		/// <param name="now">Если передать текущее время, то он будет использоваться как последнее</param>
		public static void ForceApplyUsage(ref UsageLimitModel model, DateTime? now = null)
		{
			model.usageCount++;

			if (now.HasValue)
				model.lastUsageDate = now.Value.Ticks;
		}
	}

	public struct UsageLimitApplyError
	{
		public UsageLimitEntry entry;

		public CanApplyUsageErrorCode code;
		public TimeSpan? remainingTime;

		public UsageLimitApplyError(in UsageLimitEntry entry, in CanApplyUsageErrorCode code) : this()
		{
			this.entry = entry;
			this.code = code;
		}
	}
}
