using System.Collections.Generic;
using Trading.Result;

namespace Trading.Advertising
{
	public partial class AdTokenTradeReward
	{
		public override IEnumerable<ITradeRewardResultHandle> EnumerateActualResult(Tradeboard board)
		{
			var totalCount = GetCountInternal(board);
			this.RegisterResultHandleTo(board, out AdTokenTradeRewardResultHandle handle);
			{
				handle.count = totalCount;
			}
			yield return handle;
		}
	}

	public class AdTokenTradeRewardResult : ITradeRewardResult
	{
		public int group;
		public int count;
	}

	public class AdTokenTradeRewardResultHandle : TradeRewardResultHandle<AdTokenTradeReward>
	{
		public int count;

		public override ITradeRewardResult Bake()
		{
			return new AdTokenTradeRewardResult
			{
				group = Source.group,
				count = count
			};
		}
	}
}
