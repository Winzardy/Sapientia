namespace Trading
{
	public sealed partial class TradeCostFirstAvailable : TradeCost
	{
		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			throw new System.NotImplementedException();
		}

		protected override bool Pay(Tradeboard board)
		{
			throw new System.NotImplementedException();
		}
	}
}
