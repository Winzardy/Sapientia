using System;
using Content;
using Sapientia;
using Sapientia.Evaluators;

namespace Trading.Advertising
{
	[Serializable]
	public partial class AdTokenTradeReward : TradeReward
	{
		public EvaluatedValue<Blackboard, int> count = 1;

		[ContextLabel(AdTradeReceipt.AD_TOKEN_LABEL_CATALOG)]
		public int group;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			error = null;
			return board.Contains<IAdvertisingNode>();
		}

		protected override bool Receive(Tradeboard board)
		{
			var node = board.Get<IAdvertisingNode>();
			var totalCount = GetCountInternal(board);
			node.AddToken(group, totalCount);
			this.RegisterResultHandleTo(board, out AdTokenTradeRewardResultHandle result);
			{
				result.count = totalCount;
			}
			return true;
		}

		private int GetCountInternal(Tradeboard board)
		{
			return count.Evaluate(board);
		}
	}
}
