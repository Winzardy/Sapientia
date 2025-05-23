using System;
using System.Threading;
using System.Threading.Tasks;
using Content;
using InAppPurchasing;

namespace Trading.InAppPurchasing
{
	[Serializable]
	public partial class IAPConsumableTradeCost : TradeCost
	{
		public ContentReference<IAPConsumableProductEntry> product;

		public override int Priority => TradeCostPriority.HIGH;

		public override bool CanPay(out TradePayError? error)
		{
			error = null;

			var success = IAPManager.CanPurchase(product, out var localError);

			if (localError != null)
				error = new TradePayError(TradeCostCategory.IN_APP_PURCHASING, (int) localError.Value.code, localError);

			return success;
		}

		protected override Task<bool> PayAsync(CancellationToken cancellationToken)
			=> IAPManager.PurchaseAsync(product, cancellationToken);
	}
}
