using System;
using System.Threading;
using System.Threading.Tasks;
using Content;
using InAppPurchasing;

namespace Trading.InAppPurchasing
{
	[Serializable]
	[TradeAccess(TradeAccessType.High)]
	public partial class IAPConsumableTradeCost : TradeCost
	{
		private const string ERROR_CATEGORY = "InAppPurchasing";

		public override bool Prepayment => true;

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

		protected override async Task<bool> PayAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			var result = await IAPManager.PurchaseAsync(product, cancellationToken);
			if (result.success)
				board.Register(result.receipt);
			return result.success;
		}
	}
}
