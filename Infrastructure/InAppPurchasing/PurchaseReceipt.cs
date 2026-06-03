namespace InAppPurchasing
{
	public struct PurchaseReceipt
	{
		public IAPProductType productType;
		public string productId;

		public IAPBillingEntry billing;

		public string transactionId;
		public string receipt;

		public bool isRestored;
	}
}
