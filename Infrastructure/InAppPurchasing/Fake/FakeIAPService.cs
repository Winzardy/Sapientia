using System;
using System.Collections.Generic;

namespace InAppPurchasing.Fake
{
	public class FakeIAPService : IInAppPurchasingService
	{
		public const bool DEFAULT_USE_FAKE_RESTORE_TRANSACTIONS = false;

		private SubscriptionInfo _emptySubscriptionInfo;

		public string Name => "Fake";

		public event PurchaseCompleted PurchaseCompleted;
		public event PurchaseFailed PurchaseFailed;
		public event PurchaseRequested PurchaseRequested;
		public event PurchaseCanceled PurchaseCanceled;
		public event PromotionalPurchaseIntercepted PromotionalPurchaseIntercepted;

		private Dictionary<IAPProductEntry, FakeProduct> _dictionary = new(2);

		public bool TryGetStatus(IAPProductEntry product, out ProductStatus status)
		{
			status = ProductStatus.Available;
			return true;
		}

		public bool IsRestoreTransactionsSupported { get; set; } = DEFAULT_USE_FAKE_RESTORE_TRANSACTIONS;

		public void RestoreTransactions()
		{
			IAPDebug.Log("Restored transactions");
		}

		public ref readonly ProductInfo GetProductInfo(IAPProductEntry entry, bool forceUpdateCache = false)
		{
			if (!_dictionary.TryGetValue(entry, out var product))
				product = new FakeProduct(entry);

			return ref product.info;
		}

		public bool CanPurchaseConsumable(IAPProductEntry product, out IAPPurchaseError? error)
		{
			error = null;
			return true;
		}

		public bool RequestPurchaseConsumable(IAPProductEntry entry) => Purchase(entry);

		public bool CanPurchaseNonConsumable(IAPProductEntry entry, out IAPPurchaseError? error)
		{
			if (_dictionary.TryGetValue(entry, out var product))
			{
				if (product.purchaseCount > 0)
				{
					error = IAPPurchaseErrorCode.Purchased;
					return false;
				}
			}

			error = null;
			return true;
		}

		public bool RequestPurchaseNonConsumable(IAPProductEntry entry)
		{
			if (!_dictionary.TryGetValue(entry, out var product))
				product = new FakeProduct(entry);

			product.subscriptionExpirationTime = TimeSpan.FromSeconds(600);
			return Purchase(entry);
		}

		public bool CanPurchaseSubscription(IAPProductEntry entry, out IAPPurchaseError? error)
		{
			if (_dictionary.TryGetValue(entry, out var product))
			{
				if (product.lastPurchaseTime + product.subscriptionExpirationTime > DateTime.Now)
				{
					error = IAPPurchaseErrorCode.Purchased;
					return false;
				}
			}

			error = null;
			return true;
		}

		public bool RequestPurchaseSubscription(IAPProductEntry entry) => Purchase(entry);

		public ref readonly SubscriptionInfo GetSubscriptionInfo(IAPSubscriptionProductEntry subscription, bool forceUpdateCache = false)
		{
			return ref _emptySubscriptionInfo;
		}

		private bool Purchase(IAPProductEntry entry)
		{
			PurchaseRequested?.Invoke(entry);

			if (!_dictionary.TryGetValue(entry, out var product))
				product = new FakeProduct(entry);

			product.purchaseCount++;
			product.lastPurchaseTime = DateTime.Now;

			PurchaseCompleted?.Invoke(new PurchaseReceipt
			{
				product = entry,
				receipt = product.receipt,
				transactionId = product.transactionId
			});
			return true;
		}

		private class FakeProduct
		{
			public ProductInfo info;

			public int purchaseCount;
			public DateTime lastPurchaseTime;

			public TimeSpan subscriptionExpirationTime;
			public string receipt => $"Product: {info.id}, time: {lastPurchaseTime}";
			public string transactionId => lastPurchaseTime.Ticks.ToString();

			public FakeProduct(IAPProductEntry entry)
			{
				info = new ProductInfo(entry.Id, entry.Type, entry.price);
			}
		}
	}
}
