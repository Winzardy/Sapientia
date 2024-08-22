#if UNITY_EDITOR

using System.IO;
using UnityEditor;

namespace Sapientia.TypeIndexer
{
	public static class UnityTypeIndexerAutoGenerator
	{
		[MenuItem("Code Gen/Generate Type Indexes")]
		private static void GenerateTypeIndex()
		{
			var indexesFolder = $"Assets/GeneratedScripts/{nameof(TypeIndexerGenerator)}";
			TypeIndexerGenerator.GenerateTypeIndexes(indexesFolder);

			var proxiesFolder = Path.Combine(indexesFolder, nameof(InterfaceProxyGenerator));
			InterfaceProxyGenerator.GenerateProxies(proxiesFolder);

			AssetDatabase.Refresh();
		}
	}
}

#endif
