using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sapientia
{
	public static class GitUtility
	{
		public static string? GetBranch(string projectRoot)
		{
			return RunGit(projectRoot, "rev-parse --abbrev-ref HEAD")?.Trim();
		}

		public static string? GetLastCommit(string projectRoot)
		{
			return RunGit(projectRoot, "rev-parse --short HEAD")?.Trim();
		}

		public static Dictionary<string, string> GetSubmodules(string projectRoot)
		{
			// result example:
			// d9ba378 Assets/Submodules/Sapientia
			string? output = RunGit(projectRoot, "submodule foreach -q --recursive \"echo $(git rev-parse --short HEAD) $name\"")?.Trim();

			var submodules = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(output))
			{
				var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

				foreach (var line in lines)
				{
					var parts = line.Trim().Split(' ', 2);

					if (parts.Length == 2)
					{
						string sha = parts[0];
						string name = parts[1].Trim().Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];
						submodules[name] = sha;
					}
				}
			}

			return submodules;
		}

#if UNITY_EDITOR
		public static string GetProjectRoot()
		{
			var projectRoot = System.IO.Directory.GetParent(UnityEngine.Application.dataPath)?.FullName;
			if (string.IsNullOrEmpty(projectRoot))
				throw new Exception("Can't resolve project root");

			return projectRoot!;
		}
#endif

		private static string? RunGit(string projectRoot, string args)
		{
			var psi = new ProcessStartInfo
			{
				FileName = "git",
				Arguments = args,
				WorkingDirectory = projectRoot,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true,
			};

			using var process = Process.Start(psi);
			if (process == null)
				throw new Exception("Failed to start git process");

			var stdout = process.StandardOutput.ReadToEnd();
			var stderr = process.StandardError.ReadToEnd();
			process.WaitForExit(10000);

			if (process.ExitCode != 0)
				throw new Exception($"git {args} failed: {stderr}");

			return stdout;
		}
	}
}
