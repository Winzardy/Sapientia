using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sapientia.Extensions.Reflection;
using Sapientia.MemoryAllocator;
using Sapientia.MemoryAllocator.State;

namespace Sapientia.TypeIndexer
{
	/// <summary>
	/// Editor-генератор копиров компонентов (opt-out). Рефлексией проходит по всем <see cref="IComponent"/>:
	/// плоский компонент копируется значением прямо в диспатче; ссылочный с меткой
	/// <see cref="GenerateCopyAttribute"/> получает partial <see cref="ICopiable{T}"/>; <see cref="ManualCopyAttribute"/>
	/// идёт в диспатч без генерации partial; <see cref="SkipCopyAttribute"/> исключается; ссылочный без метки
	/// попадает в лог-отчёт (worklist).
	/// </summary>
	public static class ComponentCopyGenerator
	{
		private const string GeneratorName = nameof(ComponentCopyGenerator);

		private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static readonly HashSet<Type> MemCollectionDefinitions = new()
		{
			typeof(MemArray<>), typeof(MemList<>), typeof(MemHashSet<>), typeof(MemDictionary<,>), typeof(MemSparseSet<>),
		};

		private enum FieldKind
		{
			Flat,
			EntityLink,
			EntityOwned,
			SetOwned,
			SetLink,
			IgnoreField,
		}

		private enum Verdict
		{
			Skip,
			ManualDispatch,
			FlatCopy,
			Generate,
			WorklistRefs,
			WorklistUnhandleable,
			ErrorMarkedButUnhandleable,
		}

		private readonly struct FieldPlan
		{
			public readonly string name;
			public readonly FieldKind kind;

			public FieldPlan(string name, FieldKind kind)
			{
				this.name = name;
				this.kind = kind;
			}
		}

		public static void GenerateCopiers(string baseGenerationFolder, Dictionary<string, string> assemblyNameToPath)
		{
			var report = new StringBuilder();
			var generatedByAssembly = new Dictionary<string, List<(Type type, List<FieldPlan> plans)>>();

			var flatComponents = new List<Type>();
			var refComponents = new List<Type>();

			foreach (var type in GetComponentTypes())
			{
				var verdict = Classify(type, out var plans, out var reason);
				report.AppendLine($"{verdict}\t{type.GetFullName()}{(reason != null ? "\t" + reason : string.Empty)}");

				switch (verdict)
				{
					case Verdict.FlatCopy:
						flatComponents.Add(type);
						break;
					case Verdict.ManualDispatch:
						refComponents.Add(type);
						break;
					case Verdict.Generate:
						refComponents.Add(type);
						var assemblyName = type.Assembly.GetName().Name;
						if (!generatedByAssembly.TryGetValue(assemblyName, out var list))
						{
							list = new List<(Type, List<FieldPlan>)>();
							generatedByAssembly.Add(assemblyName, list);
						}
						list.Add((type, plans));
						break;
				}
			}

			foreach (var (assemblyName, list) in generatedByAssembly)
			{
				if (!assemblyNameToPath.TryGetValue(assemblyName, out var assemblyDir))
				{
					report.AppendLine($"NoAssemblyPath\t{assemblyName}\tпропущено {list.Count} компонентов: некуда положить partial");
					continue;
				}

				var directory = PrepareDirectory(assemblyDir);
				Directory.CreateDirectory(directory);
				foreach (var (type, plans) in list)
				{
					File.WriteAllText(Path.Combine(directory, $"{type.GetFullName()}.Copy.generated.cs"), EmitComponentPartial(type, plans));
				}
			}

			var baseDirectory = Path.Combine(baseGenerationFolder, GeneratorName);
			if (Directory.Exists(baseDirectory))
				Directory.Delete(baseDirectory, true);
			Directory.CreateDirectory(baseDirectory);

			File.WriteAllText(Path.Combine(baseDirectory, "GeneratedComponentCopier.generated.cs"), EmitDispatcher(flatComponents, refComponents));
			File.WriteAllText(Path.Combine(baseDirectory, "_worklist.txt"), report.ToString());
		}

		private static string PrepareDirectory(string assemblyDir)
		{
			var directory = Path.Combine(Path.Combine(assemblyDir, "_scripts.generated"), GeneratorName);
			if (Directory.Exists(directory))
				Directory.Delete(directory, true);
			return directory;
		}

		private static Type[] GetComponentTypes()
		{
			return AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(SafeGetTypes)
				.Where(type => type.IsValueType
					&& !type.IsAbstract
					&& !type.IsGenericType
					&& type != typeof(IgnoreEntityCopy)
					&& typeof(IComponent).IsAssignableFrom(type))
				.OrderBy(type => type.GetFullName())
				.ToArray();
		}

		private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException exception)
			{
				return exception.Types.Where(type => type != null).Select(type => type!);
			}
		}

		private static Verdict Classify(Type type, out List<FieldPlan> plans, out string? reason)
		{
			plans = new List<FieldPlan>();
			reason = null;

			if (type.HasAttribute<SkipCopyAttribute>())
				return Verdict.Skip;
			if (type.HasAttribute<ManualCopyAttribute>())
				return Verdict.ManualDispatch;

			var marked = type.HasAttribute<GenerateCopyAttribute>();

			if (type.IsExplicitLayout)
			{
				reason = "explicit-layout (union)";
				return marked ? Verdict.ErrorMarkedButUnhandleable : Verdict.WorklistUnhandleable;
			}

			var hasRefs = false;
			foreach (var field in type.GetFields(InstanceFields))
			{
				if (!TryClassifyField(field, out var plan, out var fieldReason))
				{
					plans.Clear();
					reason = fieldReason;
					return marked ? Verdict.ErrorMarkedButUnhandleable : Verdict.WorklistUnhandleable;
				}

				if (plan.kind != FieldKind.Flat && plan.kind != FieldKind.IgnoreField)
					hasRefs = true;
				plans.Add(plan);
			}

			if (!hasRefs)
				return Verdict.FlatCopy;

			if (marked)
				return Verdict.Generate;

			reason = "handleable-with-refs";
			return Verdict.WorklistRefs;
		}

		private static bool TryClassifyField(FieldInfo field, out FieldPlan plan, out string? unhandleableReason)
		{
			plan = default;
			unhandleableReason = null;
			var fieldType = field.FieldType;

			if (field.GetCustomAttribute<IgnoreCopyAttribute>() != null)
			{
				plan = new FieldPlan(field.Name, FieldKind.IgnoreField);
				return true;
			}
			if (fieldType == typeof(Entity))
			{
				var owned = field.GetCustomAttribute<OwnedAttribute>() != null;
				plan = new FieldPlan(field.Name, owned ? FieldKind.EntityOwned : FieldKind.EntityLink);
				return true;
			}
			if (IsMemHashSetOfEntity(fieldType))
			{
				var link = field.GetCustomAttribute<LinkAttribute>() != null;
				plan = new FieldPlan(field.Name, link ? FieldKind.SetLink : FieldKind.SetOwned);
				return true;
			}
			if (IsFlat(fieldType))
			{
				plan = new FieldPlan(field.Name, FieldKind.Flat);
				return true;
			}

			unhandleableReason = $"{field.Name}: {fieldType.Name}";
			return false;
		}

		private static bool IsMemHashSetOfEntity(Type type)
		{
			return type.IsGenericType
				&& type.GetGenericTypeDefinition() == typeof(MemHashSet<>)
				&& type.GetGenericArguments()[0] == typeof(Entity);
		}

		private static bool IsMemCollection(Type type)
		{
			return type.IsGenericType && MemCollectionDefinitions.Contains(type.GetGenericTypeDefinition());
		}

		private static bool IsFlat(Type type)
		{
			return !ContainsEntity(type, new HashSet<Type>()) && !ContainsArenaHandle(type, new HashSet<Type>());
		}

		private static bool ContainsEntity(Type type, HashSet<Type> visited)
		{
			if (type == typeof(Entity))
				return true;
			if (type.IsPrimitive || type.IsEnum || type.IsPointer)
				return false;
			if (!visited.Add(type))
				return false;
			if (IsMemCollection(type))
				return type.GetGenericArguments().Any(argument => ContainsEntity(argument, visited));
			if (!type.IsValueType)
				return false;

			try
			{
				foreach (var field in type.GetFields(InstanceFields))
				{
					if (ContainsEntity(field.FieldType, visited))
						return true;
				}
			}
			catch
			{
				return false;
			}
			return false;
		}

		private static bool ContainsArenaHandle(Type type, HashSet<Type> visited)
		{
			if (IsArenaHandleLeaf(type) || IsMemCollection(type))
				return true;
			if (type.IsPrimitive || type.IsEnum || type.IsPointer)
				return false;
			if (!visited.Add(type))
				return false;
			if (!type.IsValueType)
				return false;

			try
			{
				foreach (var field in type.GetFields(InstanceFields))
				{
					if (ContainsArenaHandle(field.FieldType, visited))
						return true;
				}
			}
			catch
			{
				return true;
			}
			return false;
		}

		private static bool IsArenaHandleLeaf(Type type)
		{
			var name = type.Name;
			return name == "MemPtr"
				|| name.StartsWith("SafePtr")
				|| name.StartsWith("CachedPtr")
				|| name.StartsWith("ProxyPtr")
				|| name.StartsWith("IndexedPtr");
		}

		private static string EmitComponentPartial(Type type, List<FieldPlan> plans)
		{
			var name = type.Name;
			var builder = new StringBuilder();
			builder.AppendLine("using Sapientia.Collections;");
			builder.AppendLine("using Sapientia.MemoryAllocator;");
			builder.AppendLine("using Sapientia.MemoryAllocator.State;");
			builder.AppendLine();
			builder.AppendLine($"namespace {type.Namespace}");
			builder.AppendLine("{");
			builder.AppendLine($"\tpublic partial struct {name} : ICopiable<{name}>");
			builder.AppendLine("\t{");

			builder.AppendLine("\t\tpublic void AppendEntities(WorldState world, ref UnsafeList<Entity> entities)");
			builder.AppendLine("\t\t{");
			foreach (var plan in plans)
			{
				switch (plan.kind)
				{
					case FieldKind.EntityOwned:
						builder.AppendLine($"\t\t\tentities.Add({plan.name});");
						break;
					case FieldKind.SetOwned:
						builder.AppendLine($"\t\t\tforeach (var __entity in {plan.name}.GetEnumerable(world))");
						builder.AppendLine("\t\t\t{");
						builder.AppendLine("\t\t\t\tentities.Add(__entity);");
						builder.AppendLine("\t\t\t}");
						break;
				}
			}
			builder.AppendLine("\t\t}");
			builder.AppendLine();

			builder.AppendLine($"\t\tpublic void InnerCopy(WorldState oldWS, WorldState newWS, ref {name} component, in UnsafeDictionary<Entity, Entity> map)");
			builder.AppendLine("\t\t{");
			foreach (var plan in plans)
			{
				switch (plan.kind)
				{
					case FieldKind.EntityLink:
					case FieldKind.EntityOwned:
						builder.AppendLine($"\t\t\tcomponent.{plan.name} = map.Remap({plan.name});");
						break;
					case FieldKind.IgnoreField:
						builder.AppendLine($"\t\t\tcomponent.{plan.name} = default;");
						break;
					case FieldKind.SetOwned:
					case FieldKind.SetLink:
						builder.AppendLine($"\t\t\tif ({plan.name}.IsCreated)");
						builder.AppendLine("\t\t\t{");
						builder.AppendLine($"\t\t\t\tcomponent.{plan.name} = new MemHashSet<Entity>(newWS, {plan.name}.Count);");
						builder.AppendLine($"\t\t\t\tforeach (var __entity in {plan.name}.GetEnumerable(oldWS))");
						builder.AppendLine("\t\t\t\t{");
						builder.AppendLine("\t\t\t\t\tif (map.TryGetValue(__entity, out var __mapped))");
						builder.AppendLine("\t\t\t\t\t{");
						builder.AppendLine($"\t\t\t\t\t\tcomponent.{plan.name}.Add(newWS, __mapped);");
						builder.AppendLine("\t\t\t\t\t}");
						builder.AppendLine("\t\t\t\t}");
						builder.AppendLine("\t\t\t}");
						break;
				}
			}
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("}");

			return builder.ToString();
		}

		private static string EmitDispatcher(List<Type> flatComponents, List<Type> refComponents)
		{
			var builder = new StringBuilder();
			builder.AppendLine("using Sapientia.Collections;");
			builder.AppendLine("using Sapientia.MemoryAllocator;");
			builder.AppendLine("using Sapientia.MemoryAllocator.State;");
			builder.AppendLine("using Sapientia.TypeIndexer;");
			builder.AppendLine();
			builder.AppendLine("namespace Sapientia.TypeIndexer.Generated");
			builder.AppendLine("{");
			builder.AppendLine("\tpublic sealed class GeneratedComponentCopier : IComponentCopier");
			builder.AppendLine("\t{");
			builder.AppendLine("\t\tprivate static System.Collections.Generic.HashSet<int> _copiable;");
			builder.AppendLine();
			builder.AppendLine("\t\t[UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]");
			builder.AppendLine("\t\tprivate static void Register()");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tGeneratedCopier.SetCopier(new GeneratedComponentCopier());");
			builder.AppendLine("\t\t}");
			builder.AppendLine();

			builder.AppendLine("\t\tpublic bool IsCopiable(TypeId typeId)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\t_copiable ??= new System.Collections.Generic.HashSet<int>");
			builder.AppendLine("\t\t\t{");
			foreach (var type in flatComponents.Concat(refComponents))
			{
				builder.AppendLine($"\t\t\t\t(int)TypeIdOf<{type.GetFullName()}>.typeId,");
			}
			builder.AppendLine("\t\t\t};");
			builder.AppendLine("\t\t\treturn _copiable.Contains((int)typeId);");
			builder.AppendLine("\t\t}");
			builder.AppendLine();

			builder.AppendLine("\t\tpublic void AppendEntities(TypeId typeId, WorldState world, Entity entity, ref UnsafeList<Entity> frontier)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tvar __id = (int)typeId;");
			foreach (var type in refComponents)
			{
				var fullName = type.GetFullName();
				builder.AppendLine($"\t\t\tif (__id == (int)TypeIdOf<{fullName}>.typeId)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine($"\t\t\t\tvar __component = new ComponentSetContext<{fullName}>(world).ReadElement(entity);");
				builder.AppendLine("\t\t\t\t__component.AppendEntities(world, ref frontier);");
				builder.AppendLine("\t\t\t\treturn;");
				builder.AppendLine("\t\t\t}");
			}
			builder.AppendLine("\t\t}");
			builder.AppendLine();

			builder.AppendLine("\t\tpublic void CopyComponent(TypeId typeId, WorldState oldWS, WorldState newWS, Entity oldEntity, Entity newEntity, in UnsafeDictionary<Entity, Entity> map)");
			builder.AppendLine("\t\t{");
			builder.AppendLine("\t\t\tvar __id = (int)typeId;");
			foreach (var type in flatComponents)
			{
				var fullName = type.GetFullName();
				builder.AppendLine($"\t\t\tif (__id == (int)TypeIdOf<{fullName}>.typeId)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine($"\t\t\t\tref var __dst = ref new ComponentSetContext<{fullName}>(newWS).GetElement(newEntity);");
				builder.AppendLine($"\t\t\t\t__dst = new ComponentSetContext<{fullName}>(oldWS).ReadElement(oldEntity);");
				builder.AppendLine("\t\t\t\treturn;");
				builder.AppendLine("\t\t\t}");
			}
			foreach (var type in refComponents)
			{
				var fullName = type.GetFullName();
				builder.AppendLine($"\t\t\tif (__id == (int)TypeIdOf<{fullName}>.typeId)");
				builder.AppendLine("\t\t\t{");
				builder.AppendLine($"\t\t\t\tref var __dst = ref new ComponentSetContext<{fullName}>(newWS).GetElement(newEntity);");
				builder.AppendLine($"\t\t\t\t__dst = oldWS.Copy<{fullName}>(oldEntity, newWS, in map);");
				builder.AppendLine("\t\t\t\treturn;");
				builder.AppendLine("\t\t\t}");
			}
			builder.AppendLine("\t\t}");
			builder.AppendLine("\t}");
			builder.AppendLine("}");

			return builder.ToString();
		}
	}
}
