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
			var value = ReadEnvironmentNullable(environment);
			if (value == null)
				throw new Exception($"{environment} doesn't exist!");
			return value;
		}

		public static string? ReadEnvironmentNullable(this string environment)
		{
			return Environment.GetEnvironmentVariable(environment);
		}

		public static string ReadEnvironmentFile(this string environment)
		{
			var path = ReadEnvironment(environment);
			if (!File.Exists(path))
				throw new Exception($"{environment} contains incorrect path: {path}");
			return File.ReadAllText(path);
		}

		public static string? ReadEnvironmentFileNullable(this string environment)
		{
			var path = ReadEnvironmentNullable(environment);
			if (!File.Exists(path))
				return null;
			return File.ReadAllText(path);
		}
	}
}