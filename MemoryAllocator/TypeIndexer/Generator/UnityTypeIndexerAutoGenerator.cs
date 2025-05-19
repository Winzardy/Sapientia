#if UNITY_EDITOR

using System.IO;
using Sapientia.Extensions.Reflection;
using UnityEditor;

namespace Sapientia.TypeIndexer
{
	public static class UnityTypeIndexerAutoGenerator
	{
		[MenuItem("Tools/Type Indexes/Generate", priority = 100_000_001)]
		private static void GenerateTypeIndex()
		{
			var baseGenerationFolder = Path.Combine("Assets", "_scripts.generated");
			TypeIndexerGenerator.GenerateTypeIndexes(baseGenerationFolder);

			var assemblyNameToPath = ReflectionExt.GetAssemblyNameToPath();

			InterfaceProxyGenerator.GenerateProxies(baseGenerationFolder, assemblyNameToPath);
			AssetDatabase.Refresh();
		}
	}
}

#endif
