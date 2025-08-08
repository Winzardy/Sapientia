using System;
using System.Threading;
using System.Threading.Tasks;
using Content;
using InAppPurchasing;

namespace Trading.InAppPurchasing
{
	[Serializable]
	[TradeAccess(TradeAccessType.High)]
	public partial class IAPConsumableTradeCost : TradeCostWithReceipt<IAPTradeReceipt>, IAPTradeCost
	{
		private const string ERROR_CATEGORY = "InAppPurchasing";

		public ContentReference<IAPConsumableProductEntry> product;

		public override int Priority => TradeCostPriority.HIGH;

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;

			if (board.Contains<PurchaseReceipt>())
			{
				var receipt = board.Get<PurchaseReceipt>();
				board.SetRestoreState(receipt.isRestored);
				return true;
			}

			var success = IAPManager.CanPurchase(product, out var iapError);

			if (iapError != null)
				error = new TradePayError(ERROR_CATEGORY, (int) iapError.Value.code, iapError);

			return success;
		}

		protected override async Task<IAPTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			PurchaseReceipt? receipt = board.Contains<PurchaseReceipt>()
				? board.Get<PurchaseReceipt>()
				: null;

			if (!receipt.HasValue)
			{
				var result = await IAPManager.PurchaseAsync(product, cancellationToken);

				if (result.errorCode == PurchaseErrorCode.Canceled)
					throw new OperationCanceledException(cancellationToken);

				if (!result.success)
					return null;

				receipt = result.receipt;
			}

			return new IAPTradeReceipt(receipt.Value);
		}

		public IAPProductEntry GetProductEntry() => product.Read();

		protected override string GetReceiptKey(string _) => product.ToReceiptKey();
	}
}
