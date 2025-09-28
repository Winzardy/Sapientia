using System;

namespace Trading
{
	/// <summary>
	/// Бесплатно
	/// </summary>
	/// <remarks>По началу думал, что <c>null</c> и будет означать бесплатную цену, но в процессе
	/// эксплуатации понял что это неочевидно, может просто забыли заполнить данные!</remarks>
	[Serializable]
	public sealed partial class FreeTradeCost : TradeCost
	{
		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			error = null;
			return true;
		}

		protected override bool Pay(Tradeboard board) => true;
	}
}
