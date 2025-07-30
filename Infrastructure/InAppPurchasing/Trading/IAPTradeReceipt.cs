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

		public override string ToString() => Key;
	}

	public static class IAPTradeReceiptUtility
	{
		public static string Combine(IAPProductType type, string id) => type + "_" + id;

		public static string ToReceiptId(this IAPProductEntry entry) => Combine(entry.Type, entry.Id);

		public static string ToReceiptId<T>(this ContentReference<T> reference)
			where T : IAPProductEntry
			=> ToReceiptId(reference.Read());
	}
}
