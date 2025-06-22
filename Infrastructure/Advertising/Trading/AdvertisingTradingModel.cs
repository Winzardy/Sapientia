using System;

namespace Trading.Advertising
{
	public interface IAdvertisingTradingModel : ITradeReceiptRegistry<AdTradeReceipt>
	{
		public int TokenCount { get; }

		public void AddToken(int count);
	}

	[Serializable]
	public class AdvertisingTradingModel : IAdvertisingTradingModel
	{
		public int tokenCount;

		public int TokenCount => tokenCount;

		public AdvertisingTradingModel()
		{
			tokenCount = 0;
		}

		public AdvertisingTradingModel(AdvertisingTradingModel source)
		{
			tokenCount = source.tokenCount;
		}

		public void Register(string _, in AdTradeReceipt __)
		{
			AddToken(1);
		}

		public void AddToken(int count)
		{
			tokenCount += count;
		}

		public bool CanIssue(Tradeboard board, string _)
		{
			var cost = board.Get<RewardedAdTradeCost>();
			return tokenCount >= cost.count;
		}

		public bool Issue(Tradeboard board, string _)
		{
			var cost = board.Get<RewardedAdTradeCost>();
			tokenCount -= cost.count;
			return true;
		}
	}
}
