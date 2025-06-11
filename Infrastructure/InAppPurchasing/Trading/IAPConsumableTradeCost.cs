using System;
using Content;
using InAppPurchasing;

namespace Trading.InAppPurchasing
{
	[Serializable]
	[TradeAccess(TradeAccessType.High)]
	public partial class IAPConsumableTradeCost : TradeCost
	{
		private const string ERROR_CATEGORY = "InAppPurchasing";

		public ContentReference<IAPConsumableProductEntry> product;

		public override int Priority => TradeCostPriority.HIGH;

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			error = null;
			var id = product.Read().Id;
			var registry = board.Get<ITradeRegistry>();
			return registry.CanIssue<IAPTradeReceipt>(board, id);
		}

		protected override bool Pay(Tradeboard board)
		{
			var id = product.Read().Id;
			var registry = board.Get<ITradeRegistry>();
			return registry.Issue<IAPTradeReceipt>(board, id);
		}
	}

	public class IAPTradeReceipt : ITradeReceipt
	{
		public readonly PurchaseReceipt receipt;

		public string Id => receipt.productId;

		public IAPTradeReceipt(in PurchaseReceipt receipt)
		{
			this.receipt = receipt;
		}

		public override string ToString() => $"IAP Receipt: transactionId: {receipt.transactionId}";
	}
}
