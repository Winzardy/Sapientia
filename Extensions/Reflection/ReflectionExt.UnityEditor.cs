#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;

namespace Sapientia.Extensions.Reflection
{
	public static partial class ReflectionExt
	{
		private struct AssemblyDefinitionContainer
		{
			public string name;
		}

		private static Dictionary<string, string> _assemblyNameToPath = null;

		public static Dictionary<string, string> GetAssemblyNameToPath()
		{
			if (_assemblyNameToPath != null)
				return _assemblyNameToPath;

			var guids = AssetDatabase.FindAssets($"t: {nameof(AssemblyDefinitionAsset)}");
			_assemblyNameToPath = new Dictionary<string, string>();
			foreach (var guid in guids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (!assetPath.StartsWith("Assets"))
					continue;

				var assembly = assetPath.FromJsonFile<AssemblyDefinitionContainer>().name;
				var generationFolder = Path.GetDirectoryName(assetPath);

				_assemblyNameToPath.Add(assembly, generationFolder);
			}

			return _assemblyNameToPath;
		}

		public static string GetTypeAssemblyPath(Type type)
		{
			var assemblyToPath = GetAssemblyNameToPath();
			var assembly = type.Assembly.GetName().Name;

			return assemblyToPath.GetValueOrDefault(assembly, string.Empty);
		}
	}
}

#endif
