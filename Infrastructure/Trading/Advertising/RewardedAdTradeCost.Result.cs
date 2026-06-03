using Advertising;
using Content;
using Trading.Result;

namespace Trading.Advertising
{
	public partial class RewardedAdTradeCost
	{
		int IRewardedAdvertisingTradeCost.GetCountAndRegisterResult(Tradeboard board)
		{
			var totalCount = GetCountInternal(board);
			this.RegisterResultHandleTo(board, out RewardedAdTradeCostResultHandle handle);
			{
				handle.count = totalCount;
			}
			return totalCount;
		}
	}

	public class RewardedAdTradeCostResult : ITradeCostResult
	{
		public ContentReference<RewardedAdPlacementEntry> placementRef;
		public int count;

		public void Refund(Tradeboard board)
		{
			// Можно выдавать билетики за просмотр рекламы
		}
	}

	public class RewardedAdTradeCostResultHandle : TradeCostResultHandle<RewardedAdTradeCost>
	{
		public int count;

		public override ITradeCostResult Bake()
		{
			return new RewardedAdTradeCostResult
			{
				placementRef = Source.placement,
				count = count
			};
		}
	}
}
