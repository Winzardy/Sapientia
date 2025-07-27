using System;
using Sapientia.Extensions;

namespace Sapientia
{
	public enum CanApplyUsageErrorCode
	{
		None,

		LimitExceeded,
		Cooldown
	}

	public static class UsageLimitUtility
	{
		public static bool CanApplyUsage(in UsageLimitEntry entry, in UsageLimitModel model, DateTime now,
			out CanApplyUsageErrorCode errorCode)
		{
			errorCode = CanApplyUsageErrorCode.None;

			var lastUsageDateTime = model.lastUsageTicks.ToDateTime();

			if (entry.usageCount == 0) // unlimited
				return true;

			if (entry.IsResetState(lastUsageDateTime, now))
				return true;

			if (model.usageCount >= entry.usageCount)
			{
				errorCode = CanApplyUsageErrorCode.LimitExceeded;
				return false;
			}

			if (!entry.reset.IsEmpty() && !entry.reset.IsPassed(lastUsageDateTime, now))
			{
				errorCode = CanApplyUsageErrorCode.Cooldown;
				return false;
			}

			return true;
		}

		public static bool IsResetState(this in UsageLimitEntry entry, in UsageLimitModel model, DateTime now)
			=> IsResetState(entry, model.lastUsageTicks.ToDateTime(), now);

		public static bool IsResetState(this in UsageLimitEntry entry, DateTime at, DateTime now)
			=> !entry.fullReset.IsEmpty() && entry.fullReset.IsPassed(at, now);

		public static void ApplyUsage(ref UsageLimitModel model, in UsageLimitEntry entry, DateTime now)
		{
			if (!CanApplyUsage(in entry, in model, now, out var errorCode))
				throw new InvalidOperationException($"Cannot apply usage by error code [ {errorCode} ]");

			if (entry.IsResetState(in model, now))
				ForceReset(ref model);

			model.usageCount++;
			model.lastUsageTicks = now.Ticks;
		}

		/// <param name="now">Если передать текущее время, то он будет использоваться как последнее</param>
		public static void ForceReset(ref UsageLimitModel model, DateTime? now = null)
		{
			model.usageCount = 0;
			if (now.HasValue)
				model.lastUsageTicks = now.Value.Ticks;
		}

		/// <param name="now">Если передать текущее время, то он будет использоваться как последнее</param>
		public static void ForceApplyUsage(ref UsageLimitModel model, DateTime? now = null)
		{
			model.usageCount++;

			if (now.HasValue)
				model.lastUsageTicks = now.Value.Ticks;
		}
	}
}
