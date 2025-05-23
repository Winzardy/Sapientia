using System;
using System.Threading;
using System.Threading.Tasks;
using Content;
using Sapientia;

namespace InAppPurchasing
{
	public enum ProductStatus
	{
		None,

		Available,

		Pending,
		Deferred,

		/// <summary>
		/// Non-Consumable и Subscription
		/// </summary>
		Purchased,

		NotInitialized,
		NotFound,
		NotAvailable
	}

	public class IAPManagement : IDisposable
	{
		private IInAppPurchasingService _service;

		private readonly InAppPurchasingRelay _relay;

		internal IInAppPurchasingEvents Events => _relay;
		internal IInAppPurchasingService Service => _service;

		public IAPManagement(IInAppPurchasingService service)
		{
			_relay = new InAppPurchasingRelay();

			SetService(service);
		}

		public void Dispose() => _relay.Dispose();

		#region Can Purchase

		internal bool CanPurchase<T>(string product, out IAPPurchaseError? error) where T : IAPProductEntry
		{
			if (!ContentManager.Contains<T>(product))
			{
				error = IAPPurchaseErrorCode.ProductEntryNotFound;
				return false;
			}

			var entry = ContentManager.Get<T>(product);
			return CanPurchase(entry, out error);
		}

		internal bool CanPurchase(IAPProductType type, string product, out IAPPurchaseError? error)
		{
			IAPProductEntry entry;
			switch (type)
			{
				case IAPProductType.Consumable:
					entry = ContentManager.Get<IAPConsumableProductEntry>(product);
					return CanPurchase(entry, out error);
				case IAPProductType.NonConsumable:
					entry = ContentManager.Get<IAPNonConsumableProductEntry>(product);
					return CanPurchase(entry, out error);
				case IAPProductType.Subscription:
					entry = ContentManager.Get<IAPSubscriptionProductEntry>(product);
					return CanPurchase(entry, out error);
			}

			error = new IAPPurchaseError(IAPPurchaseErrorCode.ProductTypeNotImplemented);
			return false;
		}

		internal bool CanPurchase(IAPProductEntry entry, out IAPPurchaseError? error)
		{
			switch (entry.Type)
			{
				case IAPProductType.Consumable:
					return _service.CanPurchaseConsumable(entry, out error);
				case IAPProductType.NonConsumable:
					return _service.CanPurchaseNonConsumable(entry, out error);
				case IAPProductType.Subscription:
					return _service.CanPurchaseSubscription(entry, out error);
			}

			error = new IAPPurchaseError(IAPPurchaseErrorCode.ProductTypeNotImplemented);
			return false;
		}

		#endregion

		#region Purchase

		internal bool RequestPurchase<T>(string product) where T : IAPProductEntry
		{
			if (!ContentManager.Contains<T>(product))
				return false;

			var entry = ContentManager.Get<T>(product);
			return RequestPurchase(entry);
		}

		internal bool RequestPurchase(IAPProductType type, string product)
		{
			IAPProductEntry entry = type switch
			{
				IAPProductType.Consumable => ContentManager.Get<IAPConsumableProductEntry>(product),
				IAPProductType.NonConsumable => ContentManager.Get<IAPNonConsumableProductEntry>(product),
				IAPProductType.Subscription => ContentManager.Get<IAPSubscriptionProductEntry>(product),
				_ => null
			};

			return entry != null && RequestPurchase(entry);
		}

		internal bool RequestPurchase(IAPProductEntry entry)
		{
			return entry.Type switch
			{
				IAPProductType.Consumable => _service.RequestPurchaseConsumable(entry),
				IAPProductType.NonConsumable => _service.RequestPurchaseNonConsumable(entry),
				IAPProductType.Subscription => _service.RequestPurchaseSubscription(entry),
				_ => false
			};
		}

		#endregion

		#region Purchase Async

		internal Task<bool> PurchaseAsync<T>(string product, CancellationToken cancellationToken) where T : IAPProductEntry
		{
			if (!ContentManager.Contains<T>(product))
				return Task.FromResult(false);

			var entry = ContentManager.Get<T>(product);
			return PurchaseAsync(entry, cancellationToken);
		}

		internal Task<bool> PurchaseAsync(IAPProductType type, string product, CancellationToken cancellationToken)
		{
			IAPProductEntry entry = type switch
			{
				IAPProductType.Consumable => ContentManager.Get<IAPConsumableProductEntry>(product),
				IAPProductType.NonConsumable => ContentManager.Get<IAPNonConsumableProductEntry>(product),
				IAPProductType.Subscription => ContentManager.Get<IAPSubscriptionProductEntry>(product),
				_ => null
			};

			return entry ? PurchaseAsync(entry, cancellationToken) : Task.FromResult(false);
		}

		internal async Task<bool> PurchaseAsync(IAPProductEntry entry, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			// ReSharper disable once UseAwaitUsing
			using (cancellationToken.Register(Cancel))
			{
				// Это на случай если сервис поменяют, так как такой функционал есть
				var service = _service;

				service.PurchaseCompleted += OnCompleted;
				service.PurchaseFailed += OnFailed;
				service.PurchaseCanceled += OnCanceled;

				try
				{
					var success = entry.Type switch
					{
						IAPProductType.Consumable => service.RequestPurchaseConsumable(entry),
						IAPProductType.NonConsumable => service.RequestPurchaseNonConsumable(entry),
						IAPProductType.Subscription => service.RequestPurchaseSubscription(entry),
						_ => false
					};

					if (!success)
						return false;

					return await tcs.Task; //.ConfigureAwait(false); можно вне Unity)
				}
				finally
				{
					service.PurchaseCompleted -= OnCompleted;
					service.PurchaseFailed -= OnFailed;
					service.PurchaseCanceled -= OnCanceled;
				}

				void OnCompleted(in PurchaseReceipt receipt, object rawData) => tcs.TrySetResult(true);
				void OnFailed(IAPProductEntry product, string error, object rawData) => tcs.TrySetResult(false);
				void OnCanceled(IAPProductEntry product, object rawData) => tcs.TrySetResult(false);
			}

			void Cancel() => tcs.TrySetCanceled(cancellationToken);
		}

		#endregion

		#region Get Status

		internal ProductStatus GetStatus<T>(string product)
			where T : IAPProductEntry
		{
			if (!ContentManager.Contains<T>(product))
				return ProductStatus.NotFound;

			var entry = ContentManager.Get<T>(product);
			return GetStatus(entry);
		}

		internal ProductStatus GetStatus(IAPProductType type, string product)
		{
			IAPProductEntry entry = type switch
			{
				IAPProductType.Consumable => ContentManager.Get<IAPConsumableProductEntry>(product),
				IAPProductType.NonConsumable => ContentManager.Get<IAPNonConsumableProductEntry>(product),
				IAPProductType.Subscription => ContentManager.Get<IAPSubscriptionProductEntry>(product),
				_ => null
			};

			return entry == null ? ProductStatus.NotFound : GetStatus(entry);
		}

		internal ProductStatus GetStatus(IAPProductEntry product)
		{
			if (_service.TryGetStatus(product, out var status))
				return status;

			return ProductStatus.Available;
		}

		#endregion

		#region Restore

		internal bool IsRestoreTransactionsSupported() => _service.IsRestoreTransactionsSupported;
		internal void RestoreTransactions() => _service.RestoreTransactions();

		#endregion

		#region Subscription

		private readonly SubscriptionInfo _empty = default;

		internal ref readonly SubscriptionInfo GetSubscriptionInfo(string product, bool force = false)
		{
			if (!ContentManager.Contains<IAPSubscriptionProductEntry>(product))
				return ref _empty;

			var entry = ContentManager.Get<IAPSubscriptionProductEntry>(product);
			return ref _service.GetSubscriptionInfo(entry, force);
		}

		#endregion

		internal IInAppPurchasingService SetService(IInAppPurchasingService service)
		{
			var prev = _service;
#if DebugLog
			if (_service?.GetType() == service.GetType())
			{
				IAPDebug.LogWarning($"Same service: {_service.Name}");
				return prev;
			}
#endif
			_service = service;

			_relay.Bind(_service);

			IAPDebug.Log($"Target service: {_service.Name}");

			return prev;
		}
	}

	internal class InAppPurchasingRelay : Relay<IInAppPurchasingService>, IInAppPurchasingEvents
	{
		public event PurchaseCompleted PurchaseCompleted;
		public event PurchaseFailed PurchaseFailed;
		public event PurchaseRequested PurchaseRequested;
		public event PurchaseCanceled PurchaseCanceled;
		public event PromotionalPurchaseIntercepted PromotionalPurchaseIntercepted;

		protected override void OnBind(IInAppPurchasingService service)
		{
			service.PurchaseCompleted += OnPurchaseCompleted;
			service.PurchaseFailed += OnPurchaseFailed;
			service.PurchaseRequested += OnPurchaseRequested;
			service.PurchaseCanceled += OnPurchaseCanceled;
			service.PromotionalPurchaseIntercepted += OnPromotionalPurchaseIntercepted;
		}

		protected override void OnClear(IInAppPurchasingService service)
		{
			service.PurchaseCompleted -= OnPurchaseCompleted;
			service.PurchaseFailed -= OnPurchaseFailed;
			service.PurchaseRequested -= OnPurchaseRequested;
			service.PurchaseCanceled -= OnPurchaseCanceled;
			service.PromotionalPurchaseIntercepted -= OnPromotionalPurchaseIntercepted;
		}

		private void OnPurchaseCompleted(in PurchaseReceipt receipt, object rawData) => PurchaseCompleted?.Invoke(in receipt, rawData);

		private void OnPurchaseFailed(IAPProductEntry product, string error, object rawData) =>
			PurchaseFailed?.Invoke(product, error, rawData);

		private void OnPurchaseRequested(IAPProductEntry product) => PurchaseRequested?.Invoke(product);

		private void OnPurchaseCanceled(IAPProductEntry product, object rawData) => PurchaseCanceled?.Invoke(product, rawData);

		private void OnPromotionalPurchaseIntercepted(IAPProductEntry product, object rawData) =>
			PromotionalPurchaseIntercepted?.Invoke(product, rawData);
	}
}
