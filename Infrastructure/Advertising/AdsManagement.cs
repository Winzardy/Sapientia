using System;
using System.Threading;
using System.Threading.Tasks;
using Content;
using Sapientia;

namespace Advertising
{
	public class AdManagement : IDisposable
	{
		private IAdvertisingIntegration _integration;
		private readonly AdvertisingRelay _relay;

		internal IAdEvents Events => _relay;
		internal IAdvertisingIntegration Integration => _integration;

		public AdManagement(IAdvertisingIntegration integration)
		{
			_relay = new AdvertisingRelay();
			SetIntegration(integration);
		}

		public void Dispose()
			=> _relay.Dispose();

		#region Can Show

		internal bool CanShow<T>(string placement, out AdShowError? error)
			where T : AdPlacementEntry
		{
			if (!ContentManager.Contains<T>(placement))
			{
				error = AdShowErrorCode.NotFoundPlacementEntry;
				return false;
			}

			var entry = ContentManager.Get<T>(placement);
			return CanShow(entry, out error);
		}

		internal bool CanShow(AdPlacementType type, string placement, out AdShowError? error)
		{
			AdPlacementEntry entry;
			switch (type)
			{
				case AdPlacementType.Rewarded:
					entry = ContentManager.Get<RewardedAdPlacementEntry>(placement);
					return CanShow(entry, out error);
				case AdPlacementType.Interstitial:
					entry = ContentManager.Get<InterstitialAdPlacementEntry>(placement);
					return CanShow(entry, out error);
			}

			error = new AdShowError(AdShowErrorCode.NotImplementedPlacementType);
			return false;
		}

		internal bool CanShow(AdPlacementEntry entry, out AdShowError? error)
		{
			switch (entry.Type)
			{
				case AdPlacementType.Rewarded:
					return _integration.CanShowRewarded(entry, out error);
				case AdPlacementType.Interstitial:
					return _integration.CanShowInterstitial(entry, out error);
			}

			error = new AdShowError(AdShowErrorCode.NotImplementedPlacementType);
			return false;
		}

		#endregion

		#region Show

		/// <inheritdoc cref="Show(Advertising.AdPlacementEntry,bool)"/>
		internal bool Show<T>(string placement, bool autoLoad = true)
			where T : AdPlacementEntry
		{
			if (!ContentManager.Contains<T>(placement))
				return false;

			var entry = ContentManager.Get<T>(placement);
			return Show(entry, autoLoad);
		}

		/// <inheritdoc cref="Show(Advertising.AdPlacementEntry,bool)"/>
		internal bool Show(AdPlacementType type, string placement, bool autoLoad = true)
		{
			AdPlacementEntry entry = type switch
			{
				AdPlacementType.Rewarded => ContentManager.Get<RewardedAdPlacementEntry>(placement),
				AdPlacementType.Interstitial => ContentManager.Get<InterstitialAdPlacementEntry>(placement),
				_ => null
			};

			return entry && Show(entry, autoLoad);
		}

		/// <returns>Успешность запроса</returns>
		internal bool Show(AdPlacementEntry entry, bool autoLoad = true)
		{
			return entry.Type switch
			{
				AdPlacementType.Rewarded => _integration.ShowRewarded(new()
				{
					placement = entry,
					disableAutoLoad = !autoLoad,
					track = entry.integrationTrack
				}),
				AdPlacementType.Interstitial => _integration.ShowInterstitial(new()
				{
					placement = entry,
					disableAutoLoad = !autoLoad,
					track = entry.integrationTrack
				}),
				_ => false
			};
		}

		#endregion

		#region Show Async

		internal Task<bool> ShowAsync<T>(string placement, bool autoLoad, CancellationToken cancellationToken)
			where T : AdPlacementEntry
		{
			if (!ContentManager.Contains<T>(placement))
				return Task.FromResult(false);

			var entry = ContentManager.Get<T>(placement);
			return ShowAsync(entry, autoLoad, cancellationToken);
		}

		internal Task<bool> ShowAsync(AdPlacementType type, string placement, bool autoLoad, CancellationToken cancellationToken)
		{
			AdPlacementEntry entry = type switch
			{
				AdPlacementType.Rewarded => ContentManager.Get<RewardedAdPlacementEntry>(placement),
				AdPlacementType.Interstitial => ContentManager.Get<InterstitialAdPlacementEntry>(placement),
				_ => null
			};

			return entry ? ShowAsync(entry, autoLoad, cancellationToken) : Task.FromResult(false);
		}

		internal async Task<bool> ShowAsync(AdPlacementEntry entry, bool autoLoad, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			// ReSharper disable once UseAwaitUsing
			using (cancellationToken.Register(Cancel))
			{
				// Это на случай если интеграцию поменяют, так как такой функционал есть
				var integration = _integration;

				if (entry.Type == AdPlacementType.Rewarded)
				{
					integration.RewardedClosed += OnRewardedClosed;
					integration.RewardedDisplayFailed += OnDisplayFailed;
					integration.RewardedLoadFailed += OnRewardedLoadFailed;
				}
				else if (entry.Type == AdPlacementType.Interstitial)
				{
					integration.InterstitialClosed += OnInterstitialClosed;
					integration.InterstitialDisplayFailed += OnDisplayFailed;
					integration.InterstitialLoadFailed += OnInterstitialLoadFailed;
				}

				try
				{
					var success = entry.Type switch
					{
						AdPlacementType.Rewarded => integration.ShowRewarded(new()
						{
							placement = entry,
							disableAutoLoad = !autoLoad,
							track = entry.integrationTrack
						}),
						AdPlacementType.Interstitial => integration.ShowInterstitial(new()
						{
							placement = entry,
							disableAutoLoad = !autoLoad,
							track = entry.integrationTrack
						}),
						_ => false
					};

					if (!success)
						return false;

					return await tcs.Task; //.ConfigureAwait(false); можно вне Unity)
				}
				finally
				{
					if (entry.Type == AdPlacementType.Rewarded)
					{
						integration.RewardedClosed -= OnRewardedClosed;
						integration.RewardedDisplayFailed -= OnDisplayFailed;
						integration.RewardedLoadFailed -= OnRewardedLoadFailed;
					}
					else if (entry.Type == AdPlacementType.Interstitial)
					{
						integration.InterstitialClosed -= OnInterstitialClosed;
						integration.InterstitialDisplayFailed -= OnDisplayFailed;
						integration.InterstitialLoadFailed -= OnInterstitialLoadFailed;
					}
				}

				void OnDisplayFailed(AdPlacementEntry _, string __, object ___) => tcs.TrySetResult(false);

				void OnRewardedClosed(AdPlacementEntry _, bool full, object __) => tcs.TrySetResult(full);
				void OnRewardedLoadFailed(string _, object __) => tcs.TrySetResult(false);

				void OnInterstitialClosed(AdPlacementEntry _, object __) => tcs.TrySetResult(true);
				void OnInterstitialLoadFailed(string _, object __) => tcs.TrySetResult(false);
			}

			void Cancel() => tcs.TrySetCanceled(cancellationToken);
		}

		#endregion

		#region Load

		internal bool Load<T>(string placement)
			where T : AdPlacementEntry
		{
			if (!ContentManager.Contains<T>(placement))
				return false;

			var entry = ContentManager.Get<T>(placement);
			return Load(entry);
		}

		internal bool Load(AdPlacementType type, string placement)
		{
			AdPlacementEntry entry = type switch
			{
				AdPlacementType.Rewarded => ContentManager.Get<RewardedAdPlacementEntry>(placement),
				AdPlacementType.Interstitial => ContentManager.Get<InterstitialAdPlacementEntry>(placement),
				_ => null
			};

			return Load(entry);
		}

		internal bool Load(AdPlacementEntry entry)
		{
			return entry.Type switch
			{
				AdPlacementType.Rewarded => _integration.LoadRewarded(entry),
				AdPlacementType.Interstitial => _integration.LoadInterstitial(entry),
				_ => false
			};
		}

		#endregion

		#region Load Async

		internal Task<bool> LoadAsync<T>(string placement, CancellationToken cancellationToken)
			where T : AdPlacementEntry
		{
			if (!ContentManager.Contains<T>(placement))
				return Task.FromResult(false);

			var entry = ContentManager.Get<T>(placement);
			return LoadAsync(entry, cancellationToken);
		}

		internal Task<bool> LoadAsync(AdPlacementType type, string placement, CancellationToken cancellationToken)
		{
			AdPlacementEntry entry = type switch
			{
				AdPlacementType.Rewarded => ContentManager.Get<RewardedAdPlacementEntry>(placement),
				AdPlacementType.Interstitial => ContentManager.Get<InterstitialAdPlacementEntry>(placement),
				_ => null
			};

			return entry ? LoadAsync(entry, cancellationToken) : Task.FromResult(false);
		}

		internal async Task<bool> LoadAsync(AdPlacementEntry entry, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

			// ReSharper disable once UseAwaitUsing
			using (cancellationToken.Register(Cancel))
			{
				var integration = _integration;

				if (entry.Type == AdPlacementType.Rewarded)
				{
					if (integration.GetRewardedLoadingStatus(entry) == AdLoadingStatus.Loaded)
						return true;

					integration.RewardedLoaded += OnLoaded;
					integration.RewardedLoadFailed += OnFailed;
				}
				else if (entry.Type == AdPlacementType.Interstitial)
				{
					if (integration.GetInterstitialLoadingStatus(entry) == AdLoadingStatus.Loaded)
						return true;

					integration.InterstitialLoaded += OnLoaded;
					integration.InterstitialLoadFailed += OnFailed;
				}

				try
				{
					var success = entry.Type switch
					{
						AdPlacementType.Rewarded => integration.LoadRewarded(entry),
						AdPlacementType.Interstitial => integration.LoadInterstitial(entry),
						_ => false
					};

					if (!success)
						return false;

					return await tcs.Task; //.ConfigureAwait(false); можно вне Unity)
				}
				finally
				{
					if (entry.Type == AdPlacementType.Rewarded)
					{
						integration.RewardedLoaded -= OnLoaded;
						integration.RewardedLoadFailed -= OnFailed;
					}
					else if (entry.Type == AdPlacementType.Interstitial)
					{
						integration.InterstitialLoaded -= OnLoaded;
						integration.InterstitialLoadFailed -= OnFailed;
					}
				}

				void OnLoaded(object _) => tcs.TrySetResult(true);
				void OnFailed(string _, object __) => tcs.TrySetResult(false);
			}

			void Cancel() => tcs.TrySetCanceled(cancellationToken);
		}

		#endregion

		internal IAdvertisingIntegration SetIntegration(IAdvertisingIntegration integration)
		{
			var prev = _integration;
#if DebugLog
			if (_integration?.GetType() == integration.GetType())
			{
				AdsDebug.LogWarning($"Same integration: {_integration.Name}");
				return prev;
			}
#endif
			_integration = integration;
			_relay.Bind(integration);
			AdsDebug.Log($"Target integration: {_integration.Name}");

			return prev;
		}
	}

	internal class AdvertisingRelay : Relay<IAdvertisingIntegration>, IAdEvents
	{
		public event RewardedClicked RewardedClicked;
		public event RewardedClosed RewardedClosed;
		public event RewardedDisplayed RewardedDisplayed;
		public event RewardedDisplayFailed RewardedDisplayFailed;
		public event RewardedLoaded RewardedLoaded;
		public event RewardedLoadFailed RewardedLoadFailed;
		public event RewardedCompleted RewardedCompleted;

		public event InterstitialClicked InterstitialClicked;
		public event InterstitialClosed InterstitialClosed;
		public event InterstitialDisplayed InterstitialDisplayed;
		public event InterstitialDisplayFailed InterstitialDisplayFailed;
		public event InterstitialLoaded InterstitialLoaded;
		public event InterstitialLoadFailed InterstitialLoadFailed;

		protected override void OnBind(IAdvertisingIntegration integration)
		{
			integration.RewardedClicked += OnRewardedClicked;
			integration.RewardedClosed += OnRewardedClosed;
			integration.RewardedDisplayed += OnRewardedDisplayed;
			integration.RewardedDisplayFailed += OnRewardedDisplayFailed;
			integration.RewardedLoaded += OnRewardedLoaded;
			integration.RewardedLoadFailed += OnRewardedLoadFailed;
			integration.RewardedCompleted += OnRewardedCompleted;

			integration.InterstitialClicked += OnInterstitialClicked;
			integration.InterstitialClosed += OnInterstitialClosed;
			integration.InterstitialDisplayed += OnInterstitialDisplayed;
			integration.InterstitialDisplayFailed += OnInterstitialDisplayFailed;
			integration.InterstitialLoaded += OnInterstitialLoaded;
			integration.InterstitialLoadFailed += OnInterstitialLoadFailed;
		}

		protected override void OnClear(IAdvertisingIntegration integration)
		{
			integration.RewardedClicked -= OnRewardedClicked;
			integration.RewardedClosed -= OnRewardedClosed;
			integration.RewardedDisplayed -= OnRewardedDisplayed;
			integration.RewardedDisplayFailed -= OnRewardedDisplayFailed;
			integration.RewardedLoaded -= OnRewardedLoaded;
			integration.RewardedLoadFailed -= OnRewardedLoadFailed;
			integration.RewardedCompleted -= OnRewardedCompleted;

			integration.InterstitialClicked -= OnInterstitialClicked;
			integration.InterstitialClosed -= OnInterstitialClosed;
			integration.InterstitialDisplayed -= OnInterstitialDisplayed;
			integration.InterstitialDisplayFailed -= OnInterstitialDisplayFailed;
			integration.InterstitialLoaded -= OnInterstitialLoaded;
			integration.InterstitialLoadFailed -= OnInterstitialLoadFailed;
		}

		private void OnRewardedClicked(AdPlacementEntry placement, object rawData)
			=> RewardedClicked?.Invoke(placement, rawData);

		private void OnRewardedClosed(AdPlacementEntry placement, bool full, object rawData) =>
			RewardedClosed?.Invoke(placement, full, rawData);

		private void OnRewardedDisplayed(AdPlacementEntry placement, object rawData)
			=> RewardedDisplayed?.Invoke(placement, rawData);

		private void OnRewardedDisplayFailed(AdPlacementEntry placement, string error, object rawData)
			=> RewardedDisplayFailed?.Invoke(placement, error, rawData);

		private void OnRewardedLoaded(object rawData) => RewardedLoaded?.Invoke(rawData);
		private void OnRewardedLoadFailed(string error, object rawData) => RewardedLoadFailed?.Invoke(error, rawData);
		private void OnRewardedCompleted(AdPlacementEntry placement, object rawData) => RewardedCompleted?.Invoke(placement, rawData);

		private void OnInterstitialClicked(AdPlacementEntry placement, object rawData) => InterstitialClicked?.Invoke(placement, rawData);
		private void OnInterstitialClosed(AdPlacementEntry placement, object rawData) => InterstitialClosed?.Invoke(placement, rawData);

		private void OnInterstitialDisplayed(AdPlacementEntry placement, object rawData) =>
			InterstitialDisplayed?.Invoke(placement, rawData);

		private void OnInterstitialDisplayFailed(AdPlacementEntry placement, string error, object rawData) =>
			InterstitialDisplayFailed?.Invoke(placement, error, rawData);

		private void OnInterstitialLoaded(object rawData) => InterstitialLoaded?.Invoke(rawData);
		private void OnInterstitialLoadFailed(string error, object rawData) => InterstitialLoadFailed?.Invoke(error, rawData);
	}
}
