using System;
using System.IO;

namespace Sapientia.Extensions
{
	public static class EnvironmentExtensions
	{
		public static string ReadEnvironment(this string environment)
		{
			var value = Environment.GetEnvironmentVariable(environment);
			if (value == null)
				throw new Exception($"{environment} doesn't exist!");
			return value;
		}

		public static string ReadEnvironmentFile(this string environment)
		{
			var path = ReadEnvironment(environment);
			if (!File.Exists(path))
				throw new Exception($"{environment} contains incorrect path!");
			return File.ReadAllText(path);
		}
	}
}