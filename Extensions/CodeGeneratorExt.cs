#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sapientia.Extensions.Reflection;

namespace Sapientia.Extensions
{
	public static class CodeGeneratorExt
	{
		public const string GeneratedFolder = "_scripts.generated";

		public static string ReCreateGeneratedFolderForType(Type type, string subFolderName)
		{
			var path = ReflectionExt.GetTypeAssemblyPath(type);
			path = Path.Combine(path, GeneratedFolder);
			path = Path.Combine(path, subFolderName);

			if (Directory.Exists(path))
				Directory.Delete(path, true);
			Directory.CreateDirectory(path);

			return path;
		}

		public static string ClearDirectory(Type type, string subFolderName, params string[] extensions)
		{
			var path = ReflectionExt.GetTypeAssemblyPath(type);
			path = Path.Combine(path, GeneratedFolder);
			path = Path.Combine(path, subFolderName);

			var extensionsSet = new HashSet<string>(extensions);

			var files = Directory.GetFiles(path);
			foreach (var file in files)
			{
				var extension = Path.GetExtension(file) ?? string.Empty;
				if (!extensionsSet.Contains(extension))
					continue;
				File.Delete(file);
			}

			return path;
		}

		public static string CreateGeneratedScript(string folderPath, string fileName, StringBuilder fileData)
		{
			return CreateGeneratedScript(folderPath, fileName, fileData.ToString());
		}

		public static string CreateGeneratedScript(string folderPath, string fileName, string fileData)
		{
			var path = Path.Combine(folderPath, $"{fileName}.generated.cs");
			File.WriteAllText(path, fileData);

			return path;
		}
	}
}
#endif
