using System;
using System.Collections.Generic;
using System.Linq;
using Sapientia;

namespace ProjectInformation
{
	[Serializable]
	public struct BuildInfo
	{
		public string branch;
		public string commit;
		public SubmoduleInfo[] submodules;

		[Serializable]
		public struct SubmoduleInfo
		{
			public string name;
			public string commit;

			public SubmoduleInfo(string name, string commit)
			{
				this.name = name;
				this.commit = commit;
			}
		}

		public static BuildInfo CreateFromGit(string projectRoot)
		{
			return new BuildInfo()
			{
				branch = GitUtility.GetBranch(projectRoot),
				commit = GitUtility.GetLastCommit(projectRoot),
				submodules = GitUtility.GetSubmodules(projectRoot)
					.Select(entry => new SubmoduleInfo(entry.Key, entry.Value))
					.ToArray(),
			};
		}

		public static BuildInfo CreateUnknown()
		{
			return new BuildInfo() { branch = "unknown", commit = "unknown", submodules = Array.Empty<SubmoduleInfo>() };
		}
	}
}
