using System;
using System.Collections.Generic;

namespace InAppPurchasing.Fake
{
	public class FakeIAPService : IInAppPurchasingService
	{
		public const bool DEFAULT_USE_FAKE_RESTORE_TRANSACTIONS = false;

		private SubscriptionInfo _empty;

		public string Name => "Fake";

		public event PurchaseCompleted PurchaseCompleted;
		public event PurchaseFailed PurchaseFailed;
		public event PurchaseRequested PurchaseRequested;
		public event PurchaseCanceled PurchaseCanceled;
		public event PromotionalPurchaseIntercepted PromotionalPurchaseIntercepted;

		private Dictionary<IAPProductEntry, FakeProductData> _dictionary = new(2);

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

		public bool CanPurchaseConsumable(IAPProductEntry product, out IAPPurchaseError? error)
		{
			error = null;
			return true;
		}

		public bool RequestPurchaseConsumable(IAPProductEntry product) => Purchase(product);

		public bool CanPurchaseNonConsumable(IAPProductEntry product, out IAPPurchaseError? error)
		{
			if (_dictionary.TryGetValue(product, out var info))
			{
				if (info.purchaseCount > 0)
				{
					error = IAPPurchaseErrorCode.Purchased;
					return false;
				}
			}

			error = null;
			return true;
		}

		public bool RequestPurchaseNonConsumable(IAPProductEntry product)
		{
			if (!_dictionary.TryGetValue(product, out var data))
				data = new FakeProductData(product.Id);

			data.subscriptionExpirationTime = TimeSpan.FromSeconds(600);
			return Purchase(product);
		}

		public bool CanPurchaseSubscription(IAPProductEntry product, out IAPPurchaseError? error)
		{
			if (_dictionary.TryGetValue(product, out var info))
			{
				if (info.lastPurchaseTime + info.subscriptionExpirationTime > DateTime.Now)
				{
					error = IAPPurchaseErrorCode.Purchased;
					return false;
				}
			}

			error = null;
			return true;
		}

		public bool RequestPurchaseSubscription(IAPProductEntry product) => Purchase(product);

		public ref readonly SubscriptionInfo GetSubscriptionInfo(IAPSubscriptionProductEntry subscription, bool force = false)
		{
			return ref _empty;
		}

		private bool Purchase(IAPProductEntry product)
		{
			PurchaseRequested?.Invoke(product);

			if (!_dictionary.TryGetValue(product, out var data))
				data = new FakeProductData(product.Id);

			data.purchaseCount++;
			data.lastPurchaseTime = DateTime.Now;

			PurchaseCompleted?.Invoke(new PurchaseReceipt
			{
				product = product,
				receipt = data.receipt,
				transactionId = data.transactionId
			});
			return true;
		}

		private class FakeProductData
		{
			private readonly string _id;

			public int purchaseCount;
			public DateTime lastPurchaseTime;

			public TimeSpan subscriptionExpirationTime;
			public string receipt => $"Product: {_id}, time: {lastPurchaseTime}";
			public string transactionId => lastPurchaseTime.Ticks.ToString();

			public FakeProductData(string id) => this._id = id;
		}
	}
}
