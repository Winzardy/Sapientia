using System.Collections.Generic;
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

		public override IEnumerable<ITradeCostResultHandle> EnumerateActualResult(Tradeboard board)
		{
			var totalCount = GetCountInternal(board);
			this.RegisterResultHandleTo(board, out RewardedAdTradeCostResultHandle handle);
			{
				handle.count = totalCount;
			}
			yield return handle;
		}
	}

	public class RewardedAdTradeCostResult : ITradeCostResult
	{
		public ContentReference<RewardedAdPlacementEntry> placementRef;
		public int count;
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
