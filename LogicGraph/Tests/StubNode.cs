#if UNITY_5_4_OR_NEWER
using System;
using Sapientia.Data;
using Sapientia.LogicGraph.Data;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Минимальная нода-заглушка для тестов Фазы 2: объявляет только размеры 5 областей
	/// (<see cref="DataSizes"/>), поведения нет. Модель портов (<see cref="GetInputs"/> и т.п.) в
	/// scope-пути не вызывается, поэтому методы пустые.
	/// </summary>
	internal sealed class StubNode : INode
	{
		private readonly DataSizes _sizes;

		public StubNode(int staticSize = 0, int staticCacheSize = 0, int staticPersistentSize = 0, int instanceCacheSize = 0, int instancePersistentSize = 0)
		{
			_sizes = new DataSizes(staticSize, staticCacheSize, staticPersistentSize, instanceCacheSize, instancePersistentSize);
		}

		public NodeTypeId NodeTypeId => default;
		public DataSizes DataSizes => _sizes;

		public NodeInput[] GetInputs() => Array.Empty<NodeInput>();
		public NodeOutput[] GetOutputs() => Array.Empty<NodeOutput>();
		public NodeBody[] GetBodies() => Array.Empty<NodeBody>();
		public NodeState[] GetStates() => Array.Empty<NodeState>();

		public void SetBody(SafePtr bodyPtr) { }
		public void SetStateAndOutput(ref CompiledBlueprint compiledBlueprint, SafePtr statePtr, SafePtr<EdgeToData> outputPtr) { }
	}

	/// <summary>Хелпер сборки тестового блюпринта из stub-нод (связи портов пустые — Фаза 2 их не трогает).</summary>
	internal static class StubBlueprint
	{
		public static Blueprint Of(params INode[] nodes)
		{
			return new Blueprint
			{
				id = 1,
				version = 1,
				nodes = nodes,
			};
		}
	}
}
#endif
