using System;
using System.Collections.Generic;

namespace Targeting
{
	[Serializable]
	public struct ProjectInfo
	{
		public int buildNumber;
		public string identifier;
		public string version;

		public Dictionary<PlatformEntry, StorePlatformEntry> platformToStore;
		public Dictionary<StorePlatformEntry, string> storeToReviewLinks;
	}
}
