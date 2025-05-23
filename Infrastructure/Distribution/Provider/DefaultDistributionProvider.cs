using System;
using System.Collections.Generic;
using Distribution;
using Sapientia;

namespace Distribution
{
	[Serializable]
	public struct AppOptions
	{
		public int buildNumber;
		public string identifier;
		public string version;

		/// <summary>
		/// Заполняется из вне через Database!
		/// </summary>
		public Dictionary<PlatformEntry, StorePlatformEntry> platformToStore;

		/// <summary>
		/// Заполняется из вне через Database!
		/// </summary>
		public Dictionary<StorePlatformEntry, string> storeToReviewLinks;
	}

	public class DefaultDistributionProvider : IDistributionProvider
	{
		private ReactiveField<string> _userId;

		private AppOptions _options;

		private PlatformEntry _platform;
		private StorePlatformEntry _store;

		public IReactiveProperty<string> UserId => _userId;
		public string Identifier => _options.identifier;

		public DefaultDistributionProvider(AppOptions options, in PlatformEntry platform)
		{
			_options = options;
			_platform = platform;
			_store = options.platformToStore.TryGetValue(_platform, out var store) ? store : StorePlatformType.UNDEFINED;
		}

		public string GetReviewLink(StorePlatformEntry store) =>
			_options.storeToReviewLinks.TryGetValue(store, out var url) ? url : string.Empty;

		public string GetVersion() =>
			_options.buildNumber != 0 ? $"{_options.version} ({_options.buildNumber})" : _options.version;

		public ref readonly PlatformEntry GetPlatform() => ref _platform;
		public ref readonly StorePlatformEntry GetStorePlatform() => ref _store;

		void IDistributionProvider.SetUserId(string userId) => _userId.Set(userId, true);
	}
}
