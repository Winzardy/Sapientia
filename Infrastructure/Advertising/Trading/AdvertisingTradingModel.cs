using System;
using System.Collections.Generic;

namespace Trading.Advertising
{
	public interface IAdvertisingTradingModel : ITradeReceiptRegistry<AdTradeReceipt>
	{
		public int GetTokenCount(int group);

		public void AddToken(int group, int count);
	}

	[Serializable]
	public class AdvertisingTradingModel : IAdvertisingTradingModel
	{
		public Dictionary<int, int> groupToCount;

		public AdvertisingTradingModel()
		{
			groupToCount = new Dictionary<int, int>();
		}

		public AdvertisingTradingModel(AdvertisingTradingModel source)
		{
			groupToCount = new(source.groupToCount);
		}

		public void Register(string _, in AdTradeReceipt receipt)
		{
			AddToken(receipt.group, 1);
		}

		public int GetTokenCount(int group) => groupToCount.ContainsKey(group) ? groupToCount[group] : 0;

		public void AddToken(int group, int count)
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
	}
}
