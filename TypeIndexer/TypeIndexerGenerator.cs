using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GameLogic.Extensions;

namespace Sapientia.TypeIndexer
{
	public static class TypeIndexerGenerator
	{
		private static Type[] GetIndexedTypes()
		{
			var typesWithAttribute = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(assembly => assembly.GetTypes())
				.Where(type => type.GetCustomAttribute<IndexedTypeAttribute>(true) != null);

			var types = new HashSet<Type>();
			foreach (var type in typesWithAttribute)
			{
				if (!types.Add(type))
					continue;
				if (type.IsInterface || (type.IsClass && !type.IsSealed))
				{
					type.GetChildrenTypes(out var childrenTypes, out var childrenInterfaces);
					foreach (var child in childrenTypes)
						types.Add(child);
					foreach (var child in childrenInterfaces)
						types.Add(child);
				}
			}

			return types.ToArray();
		}

		public static void GenerateTypeIndexes(string outputPath)
		{
			var types = GetIndexedTypes();

			var sourceBuilder = new StringBuilder();
			sourceBuilder.AppendLine("using System;");
			sourceBuilder.AppendLine("using System.Collections.Generic;");
			sourceBuilder.AppendLine("");
			sourceBuilder.AppendLine("namespace Sapientia.TypeIndexer");
			sourceBuilder.AppendLine("{");
			sourceBuilder.AppendLine($"	public class IndexedTypesProvider : {nameof(IIndexedTypesProvider)}");
			sourceBuilder.AppendLine("	{");
			sourceBuilder.AppendLine("		private static readonly Dictionary<Type, int> _typeToIndex = new Dictionary<Type, int>");
			sourceBuilder.AppendLine("		{");
			sourceBuilder.AppendLine("#if UNITY_EDITOR || (DEBUG && !UNITY_5_3_OR_NEWER)");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"			{{ Type.GetType(\"{types[i].FullName}\"), {i} }},");
			}

			sourceBuilder.AppendLine("#else");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"			{{ typeof({types[i].FullName}), {i} }},");
			}

			sourceBuilder.AppendLine("#endif");
			sourceBuilder.AppendLine("		};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine("		public static readonly Type[] _indexToType = new Type[]");
			sourceBuilder.AppendLine("		{");
			sourceBuilder.AppendLine("#if UNITY_EDITOR || (DEBUG && !UNITY_5_3_OR_NEWER)");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"			Type.GetType(\"{types[i].FullName}\"),");
			}

			sourceBuilder.AppendLine("#else");

			for (var i = 0; i < types.Length; i++)
			{
				sourceBuilder.AppendLine($"			typeof({types[i].FullName}),");
			}

			sourceBuilder.AppendLine("#endif");
			sourceBuilder.AppendLine("		};");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine("		public Dictionary<Type, int> TypeToIndex => _typeToIndex;");
			sourceBuilder.AppendLine("		public Type[] IndexToType => IndexToType;");
			sourceBuilder.AppendLine();
			sourceBuilder.AppendLine("		public static void SetupProvider()");
			sourceBuilder.AppendLine("		{");
			sourceBuilder.AppendLine("			IndexedTypes.SetupProvider(new IndexedTypesProvider());");
			sourceBuilder.AppendLine("		}");
			sourceBuilder.AppendLine("	}");
			sourceBuilder.AppendLine("}");


			var directoryPath = Path.GetDirectoryName(outputPath)!;
			if (!Directory.Exists(directoryPath))
				Directory.CreateDirectory(directoryPath);
			File.WriteAllText(outputPath, sourceBuilder.ToString());
		}
	}
}
