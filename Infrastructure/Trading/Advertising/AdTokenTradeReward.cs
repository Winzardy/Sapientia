using System;
using Content;
using Sapientia;
using Sapientia.Evaluators;

namespace Trading.Advertising
{
	[Serializable]
	public partial class AdTokenTradeReward : TradeReward
	{
		public Evaluator<Blackboard, int> count = 1;

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
			node.AddToken(group, count.Get(board));
			return true;
		}
	}
}
