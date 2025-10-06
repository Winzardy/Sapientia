using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Logic
{
	public struct BlueprintCompiler
	{
		private PtrOffset<MonotonicAllocator> _allocatorOffset;

		private PtrOffset<PtrOffset<CompiledBlueprint>> _compiledBlueprints;
		private int _blueprintsCount;

		public static SafePtr<BlueprintCompiler> CompileAll(Blueprint[] blueprints)
		{
			var reservedSize = TSize<BlueprintCompiler>.size;

			var maxId = Id<Blueprint>.Invalid;
			foreach (var blueprint in blueprints)
			{
				if (blueprint.id > maxId)
					maxId = blueprint.id;
			}
			var blueprintsCount = (int)maxId + 1;
			reservedSize += TSize<PtrOffset<CompiledBlueprint>>.size * blueprintsCount;

			foreach (var blueprint in blueprints)
			{
				reservedSize += CompiledBlueprint.CalculateSizeToReserve(blueprint);
			}

			var allocatorPtr = MonotonicAllocator.Create(reservedSize);

			ref var allocator = ref allocatorPtr.Value();
			var compilerOffset = allocator.MemAlloc<BlueprintCompiler>();
			var result = allocator.GetPtr(compilerOffset);

			ref var compiler = ref result.Value();
			allocator.CreateRelativeOffset(ref compiler._allocatorOffset);
			compiler._blueprintsCount = blueprintsCount;
			compiler._compiledBlueprints = allocator.MemAlloc<PtrOffset<CompiledBlueprint>>(blueprintsCount);

			var compiledBlueprints = allocator.GetPtr(compiler._compiledBlueprints);
			foreach (var blueprint in blueprints)
			{
				var compiled = CompiledBlueprint.Compile(ref allocator, blueprint);
				compiledBlueprints[blueprint.id] = compiled;
			}

			return result;
		}

		public void Serialize(ref StreamBufferWriter stream)
		{
			// Т.к. _allocatorOffset - это смещение от себя до аллокатора,
			// то чтобы получить смещение от аллокатора до себя - нужно взять отрицательное значение
			var compilerOffset = -_allocatorOffset;
			stream.Write(compilerOffset);
			_allocatorOffset.GetRelativeAllocator().Serialize(ref stream);
		}

		public ref CompiledBlueprint GetCompiledBlueprint(Id<Blueprint> blueprintId)
		{
			ref var allocator = ref _allocatorOffset.GetRelativeAllocator();
			var blueprints = allocator.GetPtr(_compiledBlueprints);

			E.ASSERT(blueprintId.IsValid);
			var compilerOffset = blueprints[blueprintId];
			E.ASSERT(compilerOffset.isValid);
			return ref allocator.GetRef(compilerOffset);
		}

		public static SafePtr<BlueprintCompiler> Deserialize(ref StreamBufferReader stream)
		{
			var compilerOffset = stream.Read<PtrOffset<BlueprintCompiler>>();
			var allocator = MonotonicAllocator.Deserialize(ref stream);

			return allocator.Value().GetPtr(compilerOffset);
		}
	}
}
