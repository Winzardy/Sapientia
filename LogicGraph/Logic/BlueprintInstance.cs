using Sapientia.Data;
using Sapientia.MemoryAllocator;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph.Logic
{
	public struct BlueprintInstance
	{
		public int version;
		public Id<Blueprint> blueprintId;
		public Id<BlueprintInstance> instanceId;

		// Данные стейтов нод
		public CachedPtr nodesState;

		// Данные инпутов/аутпутов обновляются при каждой итерации вычислений инстанса
		public SafePtr<EdgeDataHeader> edgesData;

		public static CachedPtr<BlueprintInstance> Create(WorldState worldState, in CompiledBlueprint compiledBlueprint, Id<BlueprintInstance> instanceId)
		{
			var resultPtr = CachedPtr<BlueprintInstance>.Create(worldState);

			ref var result = ref resultPtr.GetValue(worldState);
			result.version = compiledBlueprint.version;
			result.blueprintId = compiledBlueprint.id;
			result.instanceId = instanceId;
			result.nodesState = worldState.MemAlloc(compiledBlueprint.nodesStateSize, out var nodesStatePtr);
			MemoryExt.MemCopy(compiledBlueprint.NodesStatePtr, nodesStatePtr, compiledBlueprint.nodesStateSize);

			return resultPtr;
		}

		public void BeginRun(SafePtr<EdgeDataHeader> newEdgesData)
		{
			edgesData = newEdgesData;
		}

		public void EndRun()
		{
			edgesData = default;
		}

		public void ResetEdges(in CompiledBlueprint compiledBlueprint)
		{
			MemoryExt.MemCopy(compiledBlueprint.EdgesDataPtr, edgesData, compiledBlueprint.edgesDataSize);
		}
	}
}
