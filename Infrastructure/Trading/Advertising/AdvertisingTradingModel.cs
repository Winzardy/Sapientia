using System;
using System.Collections.Generic;
using Advertising;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Trading.Advertising
{
	public interface IAdvertisingBackend : ITradeReceiptRegistry<AdTradeReceipt>
	{
		public int GetTokenCount(int group);

		internal void AddToken(int group, int count);
	}

	[Serializable]
	public struct AdPlacementModel
	{
		public UsageLimitModel usageLimit;
	}

	[Serializable]
	public class AdvertisingTradingModel : IAdvertisingBackend // TODO: Должен быть отдельный AdvertisingBackend сервис, убрать при реворке Interop
	{
		public Dictionary<int, int> groupToCount;
		public HashMap<string, AdPlacementModel> placementToUsageLimit;

		public AdvertisingTradingModel()
		{
			groupToCount = new Dictionary<int, int>();
			placementToUsageLimit = new HashMap<string, AdPlacementModel>();
		}

		public AdvertisingTradingModel(AdvertisingTradingModel source)
		{
			groupToCount = new(source.groupToCount);
			placementToUsageLimit = new(source.placementToUsageLimit);
		}

		public void Register(string _, in AdTradeReceipt receipt)
		{
			AddTokenInternal(receipt.group, 1);
			RegisterShowInternal(receipt);
		}

		public int GetTokenCount(int group) => groupToCount.ContainsKey(group) ? groupToCount[group] : 0;

		void IAdvertisingBackend.AddToken(int group, int count)
			=> AddTokenInternal(group, count);

		internal void AddTokenInternal(int group, int count)
		{
			if (groupToCount.TryAdd(group, count))
				return;

			groupToCount[group] += count;
		}

		public bool CanIssue(Tradeboard board, string _)
		{
			var cost = board.Get<RewardedAdTradeCost>();

			if (!groupToCount.TryGetValue(cost.group, out var count))
				return false;

			return count >= cost.count;
		}

		public bool Issue(Tradeboard board, string _)
		{
			var cost = board.Get<RewardedAdTradeCost>();
			groupToCount[cost.group] -= cost.count;
			return true;
		}

		public bool Contains(AdPlacementKey key)
			=> placementToUsageLimit.Contains(key);

		public ref readonly AdPlacementModel Get(AdPlacementKey key)
			=> ref placementToUsageLimit.GetOrAdd(key);

		private void RegisterShowInternal(AdTradeReceipt receipt)
		{
			if (AdManager.TryGetEntry(receipt.type, receipt.placement, out var entry))
			{
				if(entry.usageLimit.IsEmpty())
					return;

				var key = new AdPlacementKey(receipt.type, receipt.placement);
				ref var model = ref placementToUsageLimit.GetOrAdd(key);
				var dateTime = receipt.timestamp.ToDateTime();
				UsageLimitUtility.ApplyUsage(ref model.usageLimit, in entry.usageLimit, dateTime);
			}
			else
			{
				TradingDebug.LogError("Failed to apply usage limit: missing AdPlacementEntry " +
					$"(type: {receipt.type}, placement: {receipt.placement})");
			}
		}
	}
}
