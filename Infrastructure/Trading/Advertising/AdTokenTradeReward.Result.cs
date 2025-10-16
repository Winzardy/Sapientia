using Trading.Result;

namespace Trading.Advertising
{
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
