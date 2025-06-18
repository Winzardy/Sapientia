using System;
using Content;
using InAppPurchasing;

namespace Trading.InAppPurchasing
{
	[Serializable]
	public struct IAPTradeReceipt : ITradeReceipt
	{
		public PurchaseReceipt receipt;

#if NEWTONSOFT
		[Newtonsoft.Json.JsonIgnore]
#endif
		public string Key => IAPTradeReceiptUtility.Combine(receipt.productType, receipt.productId);
#if NEWTONSOFT
		[Newtonsoft.Json.JsonIgnore]
#endif
		public string Id => receipt.transactionId;

		public IAPTradeReceipt(in PurchaseReceipt receipt)
		{
			this.receipt = receipt;
		}

		void ITradeReceipt.Registry(ITradingModel model, string tradeId) => this.Registry(model, tradeId);

		public override string ToString() => $"IAP Receipt: {Key}";
	}

	public static class IAPTradeReceiptUtility
	{
		public static string Combine(IAPProductType type, string id) => type + "+" + id;

		public static string ToReceiptId(this IAPProductEntry entry) => Combine(entry.Type, entry.Id);

		public static string ToReceiptId<T>(this ContentReference<T> reference)
			where T : IAPProductEntry
			=> ToReceiptId(reference.Read());
	}
}
