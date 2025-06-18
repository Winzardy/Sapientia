using Sapientia;

namespace Targeting
{
	public class DefaultProjectDeskAttendant : IProjectDeskAttendant
	{
		private ReactiveField<string> _userId;

		private ProjectInfo _options;

		private PlatformEntry _platform;
		private StorePlatformEntry _store;

		public IReactiveProperty<string> UserId => _userId;
		public string Identifier => _options.identifier;

		public DefaultProjectDeskAttendant(ProjectInfo options, in PlatformEntry platform)
		{
			_options = options;
			if (!options.platformToStore.TryGetValue(_platform, out var store))
				store = StorePlatformType.UNDEFINED;

			_platform = platform;
			_store = store;

			ProjectDebug.Log($"Platform: {_platform}");
			ProjectDebug.Log($"Distribution (store): {_store}");
		}

		public string GetReviewLink(StorePlatformEntry store) =>
			_options.storeToReviewLinks.TryGetValue(store, out var url) ? url : string.Empty;

		public string GetVersion() =>
			_options.buildNumber != 0 ? $"{_options.version} ({_options.buildNumber})" : _options.version;

		public ref readonly PlatformEntry GetPlatform() => ref _platform;
		public ref readonly StorePlatformEntry GetStorePlatform() => ref _store;

		void IProjectDeskAttendant.SetUserId(string userId) => _userId.Set(userId, true);
	}
}
