using Content;
using InAppPurchasing;
using Trading.Result;

namespace Trading.InAppPurchasing
{
	public class IAPConsumableTradeCostResult : ITradeCostResult
	{
		public ContentReference<IAPConsumableProductEntry> product;

		public void Refund(Tradeboard board)
		{
			// Это на стороне IAP, скорее он сообщает нам что сделали refund и мы игровую награду возвращаем
		}
	}

	public class IAPConsumableTradeCostResultHandle : TradeCostResultHandle<IAPConsumableTradeCost>
	{
		public override ITradeCostResult Bake()
		{
			return new IAPConsumableTradeCostResult
			{
				product = _source.product
			};
		}
	}
}
