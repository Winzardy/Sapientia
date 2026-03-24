using System;
using System.Collections.Generic;
using Sapientia;

namespace ProjectInformation
{
	[Serializable]
	public struct BuildInfo
	{
		public string branch;
		public string commit;
		public Dictionary<string, string> submodules;

		public static BuildInfo CreateFromGit(string projectRoot)
		{
			return new BuildInfo()
			{
				branch = GitUtility.GetBranch(projectRoot),
				commit = GitUtility.GetLastCommit(projectRoot),
				submodules = GitUtility.GetSubmodules(projectRoot),
			};
		}
	}
}
