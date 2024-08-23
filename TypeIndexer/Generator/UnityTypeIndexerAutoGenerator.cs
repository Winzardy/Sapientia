#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEditorInternal;

namespace Sapientia.TypeIndexer
{
	public static class UnityTypeIndexerAutoGenerator
	{
		[MenuItem("Code Gen/Generate Type Indexes")]
		private static void GenerateTypeIndex()
		{
			var guid = AssetDatabase.FindAssets($"{nameof(Sapientia)} t: {nameof(AssemblyDefinitionAsset)}")[0];
			var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			var libraryPath = Path.GetDirectoryName(assetPath)!;

			var indexesFolder = $"Assets/GeneratedScripts/{nameof(TypeIndexerGenerator)}";
			TypeIndexerGenerator.GenerateTypeIndexes(indexesFolder);

			var proxiesFolder = Path.Combine(indexesFolder, nameof(InterfaceProxyGenerator));
			var libraryProxiesFolder = Path.Combine(Path.Combine(libraryPath, "GeneratedScripts"), nameof(InterfaceProxyGenerator));
			InterfaceProxyGenerator.GenerateProxies(proxiesFolder, libraryProxiesFolder);

			AssetDatabase.Refresh();
		}
	}
}

#endif
