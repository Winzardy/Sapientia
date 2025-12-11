using System;
using System.Collections.Generic;

namespace ProjectInformation
{
	[Serializable]
	public struct ProjectInfoConfig
	{
		public int buildNumber;
		public string identifier;
		public string version;

		public Dictionary<PlatformEntry, DistributionEntry> platformToDistribution;
		public Dictionary<DistributionEntry, string> distributionToStoreUrl;
	}
}
