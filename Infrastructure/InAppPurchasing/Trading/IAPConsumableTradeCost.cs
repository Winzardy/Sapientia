using System;
using System.Threading;
using System.Threading.Tasks;
using Content;
using InAppPurchasing;

namespace Trading.InAppPurchasing
{
	[Serializable]
	[TradeAccess(TradeAccessType.High)]
	public partial class IAPConsumableTradeCost : TradeCostWithReceipt<IAPTradeReceipt>
	{
		private const string ERROR_CATEGORY = "InAppPurchasing";

		public ContentReference<IAPConsumableProductEntry> product;

		public override int Priority => TradeCostPriority.HIGH;

		protected override string ReceiptId => product.ToReceiptId();

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;
			var success = IAPManager.CanPurchase(product, out var localError);

			if (localError != null)
				error = new TradePayError(ERROR_CATEGORY, (int) localError.Value.code, localError);

			return success;
		}

		protected override async Task<IAPTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			var result = await IAPManager.PurchaseAsync(product, cancellationToken);

			if (!result.success)
				return null;

			return new IAPTradeReceipt(in result.receipt);
		}
	}


}
