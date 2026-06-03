using System;
using Sapientia;
using Sapientia.Evaluators;
using Trading.Inventory;

namespace Trading
{
	[Serializable]
	public partial class TradeChangeProgressReward : TradeReward
	{
		public TradeProgressionKey key;
		public EvaluatedValue<Blackboard, int> count = 1;

		protected override bool Receive(Tradeboard board)
		{
			var node = board.Get<ITradingNode>();
			var value = count.Evaluate(board);
			node.ChangeProgress(key, value);
			this.RegisterResultHandleTo(board, out TradeSharedProgressRewardResultHandle handle);
			{
				handle.value = value;
			}
			return true;
		}
	}
}
