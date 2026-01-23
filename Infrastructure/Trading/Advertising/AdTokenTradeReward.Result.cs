using System;
using Trading.Result;

namespace Trading.Advertising
{
	public class AdTokenTradeRewardResult : ITradeRewardResult
	{
		[NonSerialized]
		public AdTokenTradeReward reward;

		public int group;
		public int count;

		public bool Merge(ITradeRewardResult other)
		{
			if (other is not AdTokenTradeRewardResult result)
				return false;

			if (result.group != group)
				return false;

			count += result.count;
			return true;
		}

		public void Return(Tradeboard board)
		{
			board.Get<IAdvertisingNode>().RemoveToken(group, count);
		}
	}

	public class AdTokenTradeRewardResultHandle : TradeRewardResultHandle<AdTokenTradeReward>
	{
		public int count;

		public override ITradeRewardResult Bake()
		{
			return new AdTokenTradeRewardResult
			{
				reward = _source,

				group = _source.group,
				count = count
			};
		}
	}
}
