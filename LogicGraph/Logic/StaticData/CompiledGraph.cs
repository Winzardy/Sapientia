using Sapientia.Data;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Logic
{
	public struct CompiledGraph
	{
		public PtrOffset<ArenaAllocator> allocatorOffset;

		public PtrOffset<Id<Blueprint>> blueprints;
		public int blueprintsCount;

		public Id<Blueprint> entryBlueprintId;
	}
}
