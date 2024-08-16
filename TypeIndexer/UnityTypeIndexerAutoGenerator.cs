#if UNITY_EDITOR

using UnityEditor;

namespace Sapientia.TypeIndexer
{
	public static class UnityTypeIndexerAutoGenerator
	{
		[MenuItem("Code Gen/Generate Type Indexes")]
		private static void GenerateTypeIndex()
		{
			var outputPath = "Assets/GeneratedScripts/IndexedTypesProvider.generated.cs";
			TypeIndexerGenerator.GenerateTypeIndexes(outputPath);
			AssetDatabase.Refresh(); // Обновляем Unity, чтобы он увидел новый файл
		}
	}
}

#endif
