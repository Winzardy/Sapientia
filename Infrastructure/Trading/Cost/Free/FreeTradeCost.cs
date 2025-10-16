using System;
using System.Collections.Generic;
using Trading.Result;

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

		protected override bool Pay(Tradeboard board)
		{
			this.RegisterResultHandleTo(board, out FreeTradeCostResultHandle _);
			return true;
		}

		public override IEnumerable<ITradeCostResultHandle> EnumerateActualResult(Tradeboard board)
		{
			this.RegisterResultHandleTo(board, out FreeTradeCostResultHandle handle);
			yield return handle;
		}
	}

	public class FreeTradeCostResult : ITradeCostResult
	{
	}

	public class FreeTradeCostResultHandle : TradeCostResultHandle<FreeTradeCost>
	{
		public override ITradeCostResult Bake() => new FreeTradeCostResult();
	}
}
