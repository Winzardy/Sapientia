using Unity.Burst;

namespace Sapientia.LogicGraph.Logic
{
	public static class NodeInvoker
	{
		public delegate void DoNode(ref CompiledBlueprint compiledBlueprint, NodeId nodeId);

		public static FunctionPointer<DoNode> CompileDoNode<T>() where T : unmanaged, ILogicNode
		{
			return BurstCompiler.CompileFunctionPointer<DoNode>(DoBurst<T>);
		}

		[BurstCompile]
		private static void DoBurst<T>(ref CompiledBlueprint compiledBlueprint, NodeId nodeId)where T : unmanaged, ILogicNode
		{
			ref var nodeStaticData = ref compiledBlueprint.GetNodeStaticData(nodeId);
			ref var body = ref compiledBlueprint.GetNodeBody<T>(nodeStaticData.body);

			body.DoBurst(ref compiledBlueprint, ref nodeStaticData, nodeId);
		}


	}
}
