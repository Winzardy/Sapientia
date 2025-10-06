using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;

namespace Sapientia.LogicGraph.Logic
{
	public interface ILogicNode<TInput, TOutput, TState> : ILogicNode<TInput, TOutput>
		where TInput : unmanaged
		where TOutput : unmanaged
		where TState : unmanaged
	{
		void ILogicNode.DoBurst(ref CompiledBlueprint compiledBlueprint, ref NodeHeader nodeHeader, NodeId nodeId)
		{
			ref var allocator = ref compiledBlueprint.allocatorOffset.GetRelativeAllocator();
			ref var input = ref allocator.GetRef<TInput>(compiledBlueprint.edgeToData + nodeHeader.inputs);
			ref var output = ref allocator.GetRef<TOutput>(compiledBlueprint.edgeToData + nodeHeader.outputs);

			TState state = default;
			DoBurst(ref compiledBlueprint, nodeId, input, output, state.AsSafePtr());
		}

		public void DoBurst(ref CompiledBlueprint compiledBlueprint, NodeId nodeId, in TInput input, in TOutput output, SafePtr<TState> state);

		void ILogicNode<TInput, TOutput>.DoBurst(ref CompiledBlueprint compiledBlueprint, NodeId nodeId, in TInput input, in TOutput output)
		{
			throw new System.NotImplementedException();
		}
	}

	public interface ILogicNode<TInput, TOutput> : ILogicNode
		where TInput: unmanaged
		where TOutput: unmanaged
	{
		void ILogicNode.DoBurst(ref CompiledBlueprint compiledBlueprint, ref NodeHeader nodeHeader, NodeId nodeId)
		{
			ref var allocator = ref compiledBlueprint.allocatorOffset.GetRelativeAllocator();
			ref var input = ref allocator.GetRef<TInput>(compiledBlueprint.edgeToData + nodeHeader.inputs);
			ref var output = ref allocator.GetRef<TOutput>(compiledBlueprint.edgeToData + nodeHeader.outputs);

			DoBurst(ref compiledBlueprint, nodeId, input, output);
		}

		public void DoBurst(ref CompiledBlueprint compiledBlueprint, NodeId nodeId, in TInput input, in TOutput output);
	}

	public interface ILogicNode
	{
		public void DoBurst(ref CompiledBlueprint compiledBlueprint, ref NodeHeader nodeHeader, NodeId nodeId);
	}
}
