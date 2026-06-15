#if UNITY_5_4_OR_NEWER
using System;
using Sapientia.TypeIndexer;

namespace Sapientia.LogicGraph.Tests
{
	/// <summary>
	/// Минимальная нода-заглушка для тестов: объявляет размеры трёх регионов
	/// (<see cref="DataSizes"/>: Static/Cache/InstancePersistence) и, опционально, порты In/Out
	/// (для тестов Static.Map). Поведения нет.
	/// </summary>
	internal sealed class StubNode : INode
	{
		private readonly DataSizes _sizes;
		private readonly NodeInput[] _inputs;
		private readonly NodeOutput[] _outputs;
		private readonly RuntimeType _runtimeType;
		private readonly TypeId<INodeContext>[] _contextTypes;

		public StubNode(int staticSize = 0, int cacheSize = 0, int persistanceSize = 0, NodeInput[] inputs = null, NodeOutput[] outputs = null, RuntimeType runtimeType = RuntimeType.Unmanaged, TypeId<INodeContext>[] contextTypes = null)
		{
			_sizes = new DataSizes(staticSize, cacheSize, persistanceSize);
			_inputs = inputs ?? Array.Empty<NodeInput>();
			_outputs = outputs ?? Array.Empty<NodeOutput>();
			_runtimeType = runtimeType;
			_contextTypes = contextTypes ?? Array.Empty<TypeId<INodeContext>>();
		}

		public TypeId<INode> NodeTypeId => default;
		public DataSizes DataSizes => _sizes;
		public RuntimeType RuntimeType => _runtimeType;

		public NodeInput[] GetInputs() => _inputs;
		public NodeOutput[] GetOutputs() => _outputs;
		public TypeId<INodeContext>[] GetContextTypes() => _contextTypes;
	}

	/// <summary>Тестовый ambient-контекст (unmanaged). В EditMode <b>не зарегистрирован</b> в <c>IndexedTypes</c>
	/// (<c>TypeId&lt;INodeContext&gt;.Count</c> = 0) ⇒ функциональные round-trip тесты реестра идут под <c>Assert.Ignore</c>.</summary>
	internal struct StubContext : INodeContext
	{
		public long value;
	}

	/// <summary>Хелпер сборки тестового блюпринта из stub-нод (связи — через <see cref="Blueprint.inputToOutput"/>).</summary>
	internal static class StubBlueprint
	{
		public static Blueprint Of(params INode[] nodes)
		{
			return Of(1, 1, nodes);
		}

		/// <summary>Сборка с явными id/version (для мульти-bp/будущих фаз).</summary>
		public static Blueprint Of(int id, int version, params INode[] nodes)
		{
			return new Blueprint
			{
				id = id,
				version = version,
				nodes = nodes,
			};
		}
	}
}
#endif
