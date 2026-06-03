using Content;
using Trading.Result;

namespace Trading.Inventory
{
	public class TradeChangeProgressRewardResult : ITradeRewardResult
	{
		public TradeProgressionKey key;
		public int value;

		public bool Merge(ITradeRewardResult other)
		{
			if (other is not TradeChangeProgressRewardResult result)
				return false;

			if (result.key != key)
				return false;

			value += result.value;
			return true;
		}

		public void Return(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			node.ChangeProgress(key, -value);
		}
	}

	public class TradeSharedProgressRewardResultHandle : TradeRewardResultHandle<TradeChangeProgressReward>
	{
		public int value;

		public override ITradeRewardResult Bake()
		{
			return new TradeChangeProgressRewardResult
			{
				key   = _source.key,
				value = value,
			};
		}
	}
}
