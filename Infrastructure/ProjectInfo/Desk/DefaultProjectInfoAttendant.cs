using Sapientia;

namespace ProjectInformation
{
	public class DefaultProjectInfoAttendant : IProjectInfoAttendant
	{
		private ReactiveField<string> _userId;

		private readonly ProjectInfoConfig _info;

		private readonly PlatformEntry _platform;
		private readonly DistributionEntry _store;

		public IReactiveProperty<string> UserId => _userId;
		public string Identifier => _info.identifier;

		public DefaultProjectInfoAttendant(in ProjectInfoConfig info, in PlatformEntry platform)
		{
			_info = info;
			if (!info.platformToDistribution.TryGetValue(platform, out var store))
				store = DistributionType.UNDEFINED;

			_platform = platform;
			_store = store;

			ProjectDebug.Log($"OS: {_platform}");
			ProjectDebug.Log($"Distribution: {_store}");
		}

		public string GetStoreUrl(DistributionEntry distribution) =>
			_info.distributionToStoreUrl.TryGetValue(distribution, out var url) ? url : string.Empty;

		public string GetVersion() =>
			_info.buildNumber != 0 ? $"{_info.version} ({_info.buildNumber})" : _info.version;

		public ref readonly PlatformEntry GetPlatform() => ref _platform;
		public ref readonly DistributionEntry GetDistribution() => ref _store;

		void IProjectInfoAttendant.SetUserId(string userId) => _userId.Set(userId, true);
	}
}
