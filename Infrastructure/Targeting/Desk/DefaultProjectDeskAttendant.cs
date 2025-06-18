using Sapientia;

namespace Targeting
{
	public class DefaultProjectDeskAttendant : IProjectDeskAttendant
	{
		private ReactiveField<string> _userId;

		private readonly ProjectInfo _info;

		private readonly PlatformEntry _platform;
		private readonly StorePlatformEntry _store;

		public IReactiveProperty<string> UserId => _userId;
		public string Identifier => _info.identifier;

		public DefaultProjectDeskAttendant(in ProjectInfo info, in PlatformEntry platform)
		{
			_info = info;
			if (!info.platformToStore.TryGetValue(platform, out var store))
				store = StorePlatformType.UNDEFINED;

			_platform = platform;
			_store = store;

			ProjectDebug.Log($"Platform: {_platform}");
			ProjectDebug.Log($"Distribution (store): {_store}");
		}

		public string GetReviewLink(StorePlatformEntry store) =>
			_info.storeToReviewLinks.TryGetValue(store, out var url) ? url : string.Empty;

		public string GetVersion() =>
			_info.buildNumber != 0 ? $"{_info.version} ({_info.buildNumber})" : _info.version;

		public ref readonly PlatformEntry GetPlatform() => ref _platform;
		public ref readonly StorePlatformEntry GetStorePlatform() => ref _store;

		void IProjectDeskAttendant.SetUserId(string userId) => _userId.Set(userId, true);
	}
}
