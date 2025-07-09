using System;
using Content;

namespace Trading.Advertising
{
	[Serializable]
	public partial class AdTokenTradeReward : TradeReward
	{
		public int count;

		[ContextLabel(AdTradeReceipt.AD_TOKEN_LABEL_CATALOG)]
		public int group;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			error = null;
			return board.Contains<IAdvertisingTradingModel>();
		}

		protected override bool Receive(Tradeboard board)
		{
			var model = board.Get<IAdvertisingTradingModel>();
			model.AddToken(group, count);
			return true;
		}
	}
}
