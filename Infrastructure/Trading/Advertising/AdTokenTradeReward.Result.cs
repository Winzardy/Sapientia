using Trading.Result;

namespace Trading.Advertising
{
	public class AdTokenTradeRewardResult : ITradeRewardResult
	{
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
	}

	public class AdTokenTradeRewardResultHandle : TradeRewardResultHandle<AdTokenTradeReward>
	{
		public int count;

		public override ITradeRewardResult Bake()
		{
			return new AdTokenTradeRewardResult
			{
				group = _source.group,
				count = count
			};
		}
	}
}
