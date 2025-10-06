using Sapientia.Data;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Logic
{
	public struct BlueprintInstance
	{
		public int version;

		public Id<Blueprint> blueprintId;
		public Id<BlueprintInstance> instanceId;

		public SafePtr<EdgeDataHeader> edgesData;
		public SafePtr nodesState;
	}
}
