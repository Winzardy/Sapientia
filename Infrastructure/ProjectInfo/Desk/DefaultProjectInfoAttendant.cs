using System;

namespace ProjectInformation
{
	public class DefaultProjectInfoAttendant : IProjectInfoAttendant
	{
		private string _userId;

		private readonly ProjectInfoConfig _info;

		private readonly PlatformEntry _platform;
		private readonly BuildInfo _buildInfo;
		private readonly DistributionEntry _store;

		public string UserId { get => _userId; }
		public string Identifier { get => _info.identifier; }
		public event Action UserIdChanged;

		public DefaultProjectInfoAttendant(in ProjectInfoConfig info, in PlatformEntry platform, BuildInfo buildInfo)
		{
			_info = info;
			if (!info.platformToDistribution.TryGetValue(platform, out var store))
				store = DistributionType.UNDEFINED;

			_platform  = platform;
			_buildInfo = buildInfo;
			_store     = store;

			ProjectDebug.Log($"OS: {_platform}");
			ProjectDebug.Log($"Distribution: {_store}");
		}

		public string GetStoreUrl(DistributionEntry distribution) =>
			_info.distributionToStoreUrl.TryGetValue(distribution, out var url) ? url : string.Empty;

		public string GetVersion() =>
			_info.buildNumber != 0 ? $"{_info.version} ({_info.buildNumber})" : _info.version;

		public virtual BuildInfo GetBuildInfo() => _buildInfo;
		public ref readonly PlatformEntry GetPlatform() => ref _platform;
		public ref readonly DistributionEntry GetDistribution() => ref _store;

		void IProjectInfoAttendant.SetUserId(string userId)
		{
			_userId = userId;
			UserIdChanged?.Invoke();
		}
	}
}
