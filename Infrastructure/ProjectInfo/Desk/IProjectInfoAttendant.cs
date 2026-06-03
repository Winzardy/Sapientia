using System;

namespace ProjectInformation
{
	public interface IProjectInfoAttendant
	{
		string Identifier { get; }

		string GetVersion();
		BuildInfo GetBuildInfo();
		ref readonly PlatformEntry GetPlatform();
		ref readonly DistributionEntry GetDistribution();
		string GetStoreUrl(DistributionEntry distribution);

		string UserId { get; }
		internal void SetUserId(string userId);
		event Action UserIdChanged;
	}
}
