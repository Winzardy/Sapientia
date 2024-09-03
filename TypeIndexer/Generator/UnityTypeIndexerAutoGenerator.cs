#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using Sapientia.Extensions;
using UnityEditor;
using UnityEditorInternal;

namespace Sapientia.TypeIndexer
{
	public static class UnityTypeIndexerAutoGenerator
	{
		private struct AssemblyDefinitionContainer
		{
			public string name;
		}

		[MenuItem("Code Gen/Generate Type Indexes")]
		private static void GenerateTypeIndex()
		{
			var baseGenerationFolder = Path.Combine("Assets", "_scripts.generated");
			TypeIndexerGenerator.GenerateTypeIndexes(baseGenerationFolder);

			var guids = AssetDatabase.FindAssets($"t: {nameof(AssemblyDefinitionAsset)}");
			var assemblyNameToGenerationFolder = new Dictionary<string, string>();
			foreach (var guid in guids)
			{
				var assetPath = AssetDatabase.GUIDToAssetPath(guid);
				if (!assetPath.StartsWith("Assets"))
					continue;
				var assembly = assetPath.FromJsonFile<AssemblyDefinitionContainer>().name;

				var generationFolder = Path.Combine(Path.GetDirectoryName(assetPath)!, "_scripts.generated");

				assemblyNameToGenerationFolder.Add(assembly, generationFolder);
			}

			InterfaceProxyGenerator.GenerateProxies(baseGenerationFolder, assemblyNameToGenerationFolder);
			AssetDatabase.Refresh();
		}
	}
}

#endif
