using System;
using System.Linq;
using Sapientia.Collections;

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
		public HashMap<int, int> groupToCount;

		public AdvertisingTradingModel()
		{
			groupToCount = new HashMap<int, int>();
		}

		public AdvertisingTradingModel(AdvertisingTradingModel source)
		{
			groupToCount = new(source.groupToCount);
		}

		public void Register(string _, in AdTradeReceipt receipt)
		{
			AddToken(receipt.group, 1);
		}

		public int GetTokenCount(int group) => groupToCount.Contains(group) ? groupToCount[group] : 0;

		public void AddToken(int group, int count)
		{
			if (groupToCount.Contains(group))
			{
				groupToCount[group] += count;
				TradingDebug.LogError($"{group}: +{count}");
				return;
			}

			TradingDebug.LogError($"New {group}: {count}");
			groupToCount.Add(group, count);
		}

		public bool CanIssue(Tradeboard board, string _)
		{
			var cost = board.Get<RewardedAdTradeCost>();
			if (!groupToCount.Contains(cost.group))
				return false;

			TradingDebug.LogError($"group:{cost.group} ,{groupToCount[cost.group]} : {cost.count}");
			return groupToCount[cost.group] >= cost.count;
		}

		public bool Issue(Tradeboard board, string _)
		{
			var cost = board.Get<RewardedAdTradeCost>();
			groupToCount[cost.group] -= cost.count;
			return true;
		}
	}
}
