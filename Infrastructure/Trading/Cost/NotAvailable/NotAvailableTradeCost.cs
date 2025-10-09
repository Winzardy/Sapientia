using System;

namespace Trading
{
	/// <summary>
	/// Недоступно!
	/// </summary>
	/// <remarks>По началу думал, что <c>null</c> и будет означать бесплатную цену, но в процессе
	/// эксплуатации понял что это неочевидно, может просто забыли заполнить данные!</remarks>
	[Serializable]
	public sealed partial class NotAvailableTradeCost : TradeCost
	{
		private const string CATEGORY = "NotAvailable";

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			error = new TradePayError(CATEGORY, 0);
			return false;
		}

		protected override bool Pay(Tradeboard board) => false;
	}
}
