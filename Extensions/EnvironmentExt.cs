using System;
using System.IO;

namespace Sapientia.Extensions
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#7f2af48da39e420aaf236ede17efcf6a
	/// </summary>
	public static class EnvironmentExt
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
				throw new Exception($"{environment} contains incorrect path: {path}");
			return File.ReadAllText(path);
		}
	}
}