using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.LogicGraph.Data;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph.Logic
{
	/// <summary>
	/// Статические данные блюпринта
	/// </summary>
	public struct CompiledBlueprint
	{
		public PtrOffset<MonotonicAllocator> allocatorOffset;

		public int version;
		public Id<Blueprint> id;

		public PtrOffset<NodeHeader> nodeHeaders;
		public int nodesCount;
		// Данные тел нод НЕ фиксированного размера
		/// <summary>
		/// Внутри могут находится статические данные ноды, они одинаковы для всех инстансов и не должны меняться
		/// </summary>
		public PtrOffset nodeBodies;
		public int nodeBodiesSize;

		// Ссылки на данные инпутов/аутпутов
		public PtrOffset<EdgeToData> edgeToData;
		public int edgesCount;

		// Данные инпутов/аутпутов НЕ фиксированного размера (Включая заголовки с дополнительным состоянием).
		/// <summary>
		/// Внутри находятся дефолтные значения инпутов/аутпутов нод, они уникальны для всех инстансов и могут меняться.
		/// Однако при каждой итерации вычислений инстанса эти данные заполняются дефолтными значениями.
		/// </summary>
		public PtrOffset<EdgeDataHeader> edgesData;
		public int edgesDataSize;

		// Данные стейтов нод НЕ фиксированного размера
		/// <summary>
		/// Внутри находятся дефолтные значения стейтов ноды, они уникальны для всех инстансов и могут меняться.
		/// </summary>
		public PtrOffset nodeState;
		public int nodeStateSize;

		public static int CalculateSizeToReserve(Blueprint blueprint)
		{
			var size = TSize<CompiledBlueprint>.size;

			foreach (var node in blueprint.nodes)
			{
				size += node.BodySize; // 1
				size += node.StateSize; // 2
			}
			size += TSize<NodeHeader>.size * blueprint.nodes.Length; // 3

			size += TSize<EdgeDataHeader>.size * blueprint.outputs.Length; // 4
			foreach (var output in blueprint.outputs)
				size += output.DataSize; // 4

			var edgesCount = blueprint.outputs.Length + blueprint.inputToOutput.Count;
			size += TSize<EdgeToData>.size * edgesCount; // 5

			return size;
		}

		public static PtrOffset<CompiledBlueprint> Compile(ref MonotonicAllocator allocator, Blueprint blueprint)
		{
			var result = allocator.MemAlloc<CompiledBlueprint>();
			ref var compiled = ref allocator.GetRef(result);

			allocator.CreateRelativeOffset(ref compiled.allocatorOffset);
			compiled.SetupBlueprint(blueprint);

			return result;
		}

		private void SetupBlueprint(Blueprint blueprint)
		{
			ref var allocator = ref allocatorOffset.GetRelativeAllocator();

			version = blueprint.version;
			id = blueprint.id;

			// Создаём базовое состояние блюпринта
			nodeBodiesSize = 0;
			nodeStateSize = 0;
			foreach (var node in blueprint.nodes)
			{
				nodeBodiesSize += node.BodySize;
				nodeStateSize += node.StateSize;
			}
			nodeBodies = allocator.MemAlloc(nodeBodiesSize); // 1. CalculateSizeToReserve (Аллокация)
			nodeState = allocator.MemAlloc(nodeStateSize); // 2. CalculateSizeToReserve (Аллокация)

			// Ссылки на статические данные ноды
			nodeHeaders = allocator.MemAlloc<NodeHeader>(blueprint.nodes.Length); // 3. CalculateSizeToReserve (Аллокация)
			nodesCount = blueprint.nodes.Length;

			// Ссылки на данные интупов/аутпутов
			edgesDataSize = TSize<EdgeDataHeader>.size * blueprint.outputs.Length;
			foreach (var output in blueprint.outputs)
				edgesDataSize += output.DataSize;
			edgesData = allocator.MemAlloc(edgesDataSize); // 4. CalculateSizeToReserve (Аллокация)

			// Создаём маппинг output -> оффсет до его хедера
			var edgesStateRover = edgesData;
			// Если их много, то лучше аллоцировать память не на стеке
			var outputIdToOffset = (Span<PtrOffset<EdgeDataHeader>>) stackalloc PtrOffset<EdgeDataHeader>[blueprint.outputs.Length];
			for (var i = 0; i < blueprint.outputs.Length; i++)
			{
				if (blueprint.outputs[i].IsPreCalculated)
				{
					// Устанавливаем дефолтное значение, оно всегда будет считаться вычисленным
					allocator.GetRef(edgesStateRover).IsCalculated = true;
				}
				outputIdToOffset[i] = edgesStateRover - edgesData;
				edgesStateRover++;
				edgesStateRover = edgesStateRover.Offset(blueprint.outputs[i].DataSize);
			}

			// Ссылки на данные
			edgesCount = blueprint.outputs.Length + blueprint.inputToOutput.Count;
			edgeToData = allocator.MemAlloc<EdgeToData>(edgesCount); // 5. CalculateSizeToReserve (Аллокация)

			var nodeBodiesPtr = allocator.GetPtr(nodeBodies);
			var nodeStatePtr = allocator.GetPtr(nodeState);
			var nodeStaticDataPtr = allocator.GetPtr(nodeHeaders);
			var edgeToDataPtr = allocator.GetPtr(edgeToData);

			var nodesBodyRover = nodeBodiesPtr;
			var nodeStateRover = nodeStatePtr;
			var nodeStaticDataRover = nodeStaticDataPtr;
			var edgeToDataRover = edgeToDataPtr;
			foreach (var node in blueprint.nodes)
			{
				ref var staticData = ref nodeStaticDataRover.Value();

				// Устанавливаем тип ноды
				staticData.nodeTypeId = node.NodeTypeId;

				// Устанавливаем ссылки на данные инпутов ноды
				staticData.inputs = edgeToDataRover - edgeToDataPtr;
				foreach (var input in node.GetInputs())
				{
					// Мы предполагаем, что все инпуты связаны с аутпутами (Дефолтное значение тоже должно быть аутпутом)
					var output = blueprint.inputToOutput[input];
					var outputIndex = blueprint.outputToIndexMap[output];
					var outputOffset = outputIdToOffset[outputIndex];

					edgeToDataRover.Value().dataOffset = outputOffset;
					edgeToDataRover++;
				}

				// Устанавливаем ссылки на данные аутпутов ноды
				staticData.outputs = edgeToDataRover - edgeToDataPtr;
				foreach (var output in node.GetOutputs())
				{
					var outputIndex = blueprint.outputToIndexMap[output];
					var outputOffset = outputIdToOffset[outputIndex];

					edgeToDataRover.Value().dataOffset = outputOffset;
					edgeToDataRover++;
				}

				// Устанавливаем данные тела ноды и ссылку на данные тела
				node.SetBody(nodesBodyRover);
				staticData.body = nodesBodyRover - nodeBodiesPtr;
				nodesBodyRover += node.BodySize;

				// Устанавливаем стейт ноды и аутпут
				var edgeStatePtr = (edgeToDataPtr + staticData.outputs);
				node.SetStateAndOutput(ref this, nodeStateRover, edgeStatePtr);
				staticData.state = nodeStateRover - nodeStatePtr;
				nodeStateRover += node.StateSize;
			}
		}

		public ref NodeHeader GetNodeStaticData(NodeId nodeId)
		{
			return ref allocatorOffset.GetPtr(nodeHeaders)[nodeId];
		}

		public ref T GetNodeBody<T>(PtrOffset bodyPtr)
			where T : unmanaged, ILogicNode
		{
			return ref allocatorOffset.GetPtr<T>(nodeBodies + bodyPtr).Value();
		}
	}

	public struct NodeId
	{
		public int id;

		public static implicit operator int(NodeId nodeId)
		{
			return nodeId.id;
		}

		public static implicit operator NodeId(int id)
		{
			return new NodeId() { id = id };
		}
	}

	public struct NodeHeader
	{
		public NodeTypeId nodeTypeId;

		public PtrOffset<EdgeToData> inputs;
		public int inputsCount;

		public PtrOffset<EdgeToData> outputs;
		public int outputsCount;

		public PtrOffset body;
		public PtrOffset state;
	}

	public struct EdgeToData
	{
		public PtrOffset<EdgeDataHeader> dataOffset;
	}

	public readonly struct InputData<T> where T : unmanaged
	{
		private readonly PtrOffset<EdgeDataHeader> _dataOffset;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref readonly T ReadData(ref CompiledBlueprint compiledBlueprint, SafePtr state)
		{
			ref var header = ref compiledBlueprint.allocatorOffset.GetRef<EdgeDataHeader>(compiledBlueprint.edgesData + _dataOffset);
			if (header.PassThroughRef)
			{
				var offset = UnsafeExt.As<EdgeDataHeader, EdgeData<PtrOffset<T>>>(ref header).data;
				return ref (state + offset).Value();
			}
			return ref UnsafeExt.As<EdgeDataHeader, EdgeData<T>>(ref header).data;
		}
	}

	public readonly struct OutputData<T> where T : unmanaged
	{
		private readonly PtrOffset<EdgeDataHeader> _dataOffset;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetData(ref CompiledBlueprint compiledBlueprint)
		{
			return ref compiledBlueprint.allocatorOffset.GetRef<EdgeData<T>>(compiledBlueprint.edgesData + _dataOffset).data;
		}
	}

	public readonly struct StateData<T> where T : unmanaged
	{
		private readonly PtrOffset<EdgeDataHeader> _dataOffset;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetData(ref CompiledBlueprint compiledBlueprint, SafePtr state)
		{
			var offset = GetOffset(ref compiledBlueprint);
			return ref (state + offset).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref PtrOffset<T> GetOffset(ref CompiledBlueprint compiledBlueprint)
		{
			return ref compiledBlueprint.allocatorOffset.GetRef<EdgeData<PtrOffset<T>>>(compiledBlueprint.edgesData + _dataOffset).data;
		}
	}

	public enum EdgeDataHeaderState
	{
		IsCalculated,
		PassThroughRef,
	}

	public struct EdgeDataHeader
	{
		public ByteEnumMask<EdgeDataHeaderState> state;

		public bool IsCalculated
		{
			get => state.Has(EdgeDataHeaderState.IsCalculated);
			set => state.Set(EdgeDataHeaderState.IsCalculated, value);
		}

		public bool PassThroughRef
		{
			get => state.Has(EdgeDataHeaderState.PassThroughRef);
			set => state.Set(EdgeDataHeaderState.PassThroughRef, value);
		}
	}

	public struct EdgeData<T> where T : unmanaged
	{
		public EdgeDataHeader state;
		public T data;
	}
}
