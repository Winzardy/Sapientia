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
		private const string ERROR_CATEGORY = "InAppPurchasing";

		public ContentReference<IAPConsumableProductEntry> product;

		public override int Priority => TradeCostPriority.HIGH;

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			error = null;

			var success = IAPManager.CanPurchase(product, out var localError);

			if (localError != null)
				error = new TradePayError(ERROR_CATEGORY, (int) localError.Value.code, localError);

			return success;
		}

		protected override Task<bool> PayAsync(Tradeboard board, CancellationToken cancellationToken)
			=> IAPManager.PurchaseAsync(product, cancellationToken);
	}
}
