using System;
using System.Collections.Generic;
using Content;

namespace ProjectInformation
{
	[Serializable]
	public struct ProjectInfoConfig
	{
		[ClientOnly]
		public int buildNumber;
		public string identifier;
		public string version;

		public Dictionary<PlatformEntry, DistributionEntry> platformToDistribution;
		public Dictionary<DistributionEntry, string> distributionToStoreUrl;
	}
}
