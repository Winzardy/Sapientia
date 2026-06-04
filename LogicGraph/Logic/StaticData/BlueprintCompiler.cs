using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	public struct BlueprintCompiler
	{
		private RelativePtr<BumpHeader> _allocatorRef;

		private BumpArray<RelativePtr<CompiledBlueprint>> _compiledBlueprints;

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

			var arena = new RawBumpAllocator(reservedSize);

			ref var allocator = ref arena.Value;
			var compilerOffset = allocator.MemAlloc<BlueprintCompiler>();
			var result = allocator.GetPtr(compilerOffset);

			ref var compiler = ref result.Value();
			allocator.SetupRelativePtr(ref compiler._allocatorRef);
			compiler._compiledBlueprints.Alloc(ref allocator, blueprintsCount);

			var compiledBlueprints = compiler._compiledBlueprints.GetSpan();
			foreach (var blueprint in blueprints)
			{
				var compiled = CompiledBlueprint.Compile(ref allocator, blueprint, out _);
				compiledBlueprints[blueprint.id].SetPtr(compiled);
			}

			return result;
		}

		public void Serialize(ref StreamBufferWriter stream)
		{
			// Т.к. _allocatorRef - это смещение от себя до аллокатора,
			// то чтобы получить смещение от аллокатора до себя - нужно взять отрицательное значение
			var compilerOffset = -(PtrOffset)_allocatorRef;
			stream.Write(compilerOffset);
			_allocatorRef.GetValue().Serialize(ref stream);
		}

		public ref CompiledBlueprint GetCompiledBlueprint(Id<Blueprint> blueprintId)
		{
			E.ASSERT(blueprintId.IsValid);
			var blueprints = _compiledBlueprints.GetSpan();
			return ref blueprints[blueprintId].GetValue();
		}

		public static SafePtr<BlueprintCompiler> Deserialize(ref StreamBufferReader stream)
		{
			var compilerOffset = stream.Read<PtrOffset<BlueprintCompiler>>();
			var arena = RawBumpAllocator.Deserialize(ref stream);

			return arena.Value.GetPtr(compilerOffset);
		}
	}
}
