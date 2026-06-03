using Sapientia.Data;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	public struct CompiledGraph
	{
		public PtrOffset<BumpHeader> allocatorOffset;

		public PtrOffset<Id<Blueprint>> blueprints;
		public int blueprintsCount;

		public Id<Blueprint> entryBlueprintId;
	}
}
