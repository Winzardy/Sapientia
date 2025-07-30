using System;
using System.Threading;
using System.Threading.Tasks;
using Content;

namespace Advertising
{
	public class AdManagement : IDisposable
	{
		private readonly IAdvertisingService _service;
		private IAdvertisingIntegration _integration;

		private readonly AdvertisingIntegrationRelay _relay;

		internal IAdEvents Events => _relay;
		internal IAdvertisingIntegration Integration => _integration;

		public AdManagement(IAdvertisingIntegration integration, IAdvertisingService service)
		{
			_relay = new AdvertisingIntegrationRelay();
			if (service != null)
				_relay.AdDisplayFinished += OnAdFinished;

			_service = service;
			SetIntegration(integration);
		}

		public void Dispose()
		{
			if (_service != null)
				_relay.AdDisplayFinished -= OnAdFinished;
			_relay.Dispose();
		}

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
			if (!_service.CanShow(entry, out var backendError))
			{
				error = backendError;
				return false;
			}

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
			return Load(GetEntry(type, placement));
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
#if DEBUG
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

		internal AdPlacementEntry GetEntry(AdPlacementType type, string placement)
			=> type switch
			{
				AdPlacementType.Rewarded => ContentManager.Get<RewardedAdPlacementEntry>(placement),
				AdPlacementType.Interstitial => ContentManager.Get<InterstitialAdPlacementEntry>(placement),
				_ => null
			};

		internal bool TryGetEntry(AdPlacementType type, string placement, out AdPlacementEntry entry)
		{
			entry = null;
			switch (type)
			{
				case AdPlacementType.Rewarded:
					if (ContentManager.TryGetEntry<RewardedAdPlacementEntry>(placement, out var rEntry))
					{
						entry = rEntry;
						return true;
					}

					break;
				case AdPlacementType.Interstitial:
					if (ContentManager.TryGetEntry<InterstitialAdPlacementEntry>(placement, out var iEntry))
					{
						entry = iEntry;
						return true;
					}

					break;
			}

			return false;
		}

		private void OnAdFinished(AdPlacementEntry placement, bool full)
		{
			if (full)
				_service.RegisterShow(placement);
		}
	}
}
