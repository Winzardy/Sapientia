using Sapientia.Data;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Logic
{
	public struct LogicGraph
	{
		public SafePtr<BlueprintCompiler> compiler;

		public Id<Blueprint> entryBlueprintId;
	}
}
