using System;

namespace Trading.Advertising
{
	[Serializable]
	public partial class AdTokenTradeReward : TradeReward
	{
		public int count;

		protected override bool CanReceive(Tradeboard board, out TradeReceiveError? error)
		{
			error = null;
			return board.Contains<IAdvertisingTradingModel>();
		}

		protected override bool Receive(Tradeboard board)
		{
			var model = board.Get<IAdvertisingTradingModel>();
			model.AddToken(count);
			return true;
		}
	}
}
