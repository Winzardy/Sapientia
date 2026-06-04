using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.LogicGraph.Data;
using Sapientia.Memory;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Статические данные блюпринта
	/// </summary>
	public struct CompiledBlueprint
	{
		public RelativePtr<BumpHeader> allocatorOffset;

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
		public SafePtr<EdgeDataHeader> EdgesDataPtr => allocatorOffset.GetPtr(edgesData);

		// Данные стейтов нод НЕ фиксированного размера
		/// <summary>
		/// Внутри находятся дефолтные значения стейтов ноды, они уникальны для всех инстансов и могут меняться.
		/// </summary>
		public PtrOffset nodesState;
		public int nodesStateSize;
		public SafePtr NodesStatePtr => allocatorOffset.GetPtr(nodesState);

		// ─────────────────────────── Phase 2: раскладка 5 областей ───────────────────────────
		// Независимый от модели портов (legacy-поля/методы выше) sizing-only слой. Описывает, где лежит
		// слайс каждой ноды в каждой из 5 областей. Реально аллоцируется здесь только static-блок;
		// static cache/persistent заводит scope (Фаза 3), instance cache/persistent — инстанс.

		/// <summary>Полный размер блока каждой из 5 областей (сумма выровненных слотов всех нод).</summary>
		public DataSizes blockSizes;
		/// <summary>Таблица офсетов слайса каждой ноды по всем 5 областям (живёт в static-арене).</summary>
		public BumpArray<NodeLayoutOffsets> nodeLayoutOffsets;
		/// <summary>Сам блок static-области (живёт в static-арене).</summary>
		public PtrOffset staticBlock;

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

		public static SafePtr<CompiledBlueprint> Compile(ref BumpHeader allocator, Blueprint blueprint, out PtrOffset<CompiledBlueprint> offset)
		{
			ref var compiled = ref allocator.MemAlloc(out offset, out var result);

			allocator.SetupRelativePtr(ref compiled.allocatorOffset);
			compiled.SetupBlueprint(blueprint);

			return result;
		}

		private void SetupBlueprint(Blueprint blueprint)
		{
			ref var allocator = ref allocatorOffset.GetValue();

			version = blueprint.version;
			id = blueprint.id;

			// Создаём базовое состояние блюпринта
			nodeBodiesSize = 0;
			nodesStateSize = 0;
			foreach (var node in blueprint.nodes)
			{
				nodeBodiesSize += node.BodySize;
				nodesStateSize += node.StateSize;
			}
			nodeBodies = allocator.MemAlloc(nodeBodiesSize); // 1. CalculateSizeToReserve (Аллокация)
			nodesState = allocator.MemAlloc(nodesStateSize); // 2. CalculateSizeToReserve (Аллокация)

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
					allocator.GetValue(edgesStateRover).IsCalculated = true;
				}
				outputIdToOffset[i] = edgesStateRover - edgesData;
				edgesStateRover++;
				edgesStateRover = edgesStateRover.Offset(blueprint.outputs[i].DataSize);
			}

			// Ссылки на данные
			edgesCount = blueprint.outputs.Length + blueprint.inputToOutput.Count;
			edgeToData = allocator.MemAlloc<EdgeToData>(edgesCount); // 5. CalculateSizeToReserve (Аллокация)

			var nodeBodiesPtr = allocator.GetPtr(nodeBodies);
			var nodeStatePtr = allocator.GetPtr(nodesState);
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

		// ─────────────────────────── Phase 2: компиляция раскладки 5 областей ───────────────────────────

		/// <summary>
		/// Сколько байт зарезервировать в static-арене под scope-раскладку. Держится в lockstep с
		/// <see cref="SetupLayout"/> по нумерованным шагам (1..3) — менять только вместе.
		/// </summary>
		public static int CalculateLayoutSizeToReserve(Blueprint blueprint)
		{
			var size = TSize<CompiledBlueprint>.size; // 1. сама структура

			var nodesCount = blueprint.nodes.Length;
			if (nodesCount > 0)
				size += TSize<NodeLayoutOffsets>.size * nodesCount; // 2. таблица офсетов нод

			var staticBlockSize = 0;
			foreach (var node in blueprint.nodes)
				staticBlockSize += node.DataSizes.GetAligned(DataLayout.Static);
			size += staticBlockSize; // 3. блок static-области

			return size;
		}

		/// <summary>
		/// Standalone-компиляция одного блюпринта в собственную raw-арену (sizing-only,
		/// без модели портов). Владение ареной — на вызывающем (Dispose).
		/// </summary>
		public static RawBumpAllocator CompileLayout(Blueprint blueprint, out PtrOffset<CompiledBlueprint> offset)
		{
			var reservedSize = CalculateLayoutSizeToReserve(blueprint);
			var arena = new RawBumpAllocator(reservedSize);

			ref var allocator = ref arena.Value;
			ref var compiled = ref allocator.MemAlloc(out offset); // 1. структура

			allocator.SetupRelativePtr(ref compiled.allocatorOffset);
			compiled.SetupLayout(blueprint);

			return arena;
		}

		private void SetupLayout(Blueprint blueprint)
		{
			ref var allocator = ref allocatorOffset.GetValue();

			// Ключ static-блока — (id, version).
			version = blueprint.version;
			id = blueprint.id;

			var nodes = blueprint.nodes;
			var nodeCount = nodes.Length;
			if (nodeCount == 0)
				return;

			nodeLayoutOffsets.Alloc(ref allocator, nodeCount); // 2. таблица офсетов нод (in-place, self-relative)
			var offsetsPtr = nodeLayoutOffsets.GetSpan();

			// Офсет ноды в каждой области = текущая сумма выровненных слотов (blockSizes до этой ноды).
			for (var i = 0; i < nodes.Length; i++)
			{
				ref var nodeOffsets = ref offsetsPtr[i];
				for (var l = 0; l < DataSizes.Count; l++)
					nodeOffsets[l] = new PtrOffset(blockSizes[l]);
				blockSizes += nodes[i].DataSizes.GetAligned();
			}

			// 3. Сам блок static-области (остальные 4 области заводят их владельцы — scope/instance).
			var staticBlockSize = blockSizes[DataLayout.Static];
			if (staticBlockSize > 0)
				staticBlock = allocator.MemAlloc(staticBlockSize);
		}

		public int GetBlockSize(DataLayout scope)
		{
			return blockSizes[scope];
		}

		/// <summary>Офсет слайса ноды внутри блока заданной области.</summary>
		/// <remarks>Использует self-relative <see cref="allocatorOffset"/> — вызывать только через
		/// ref/арена-указатель; на копии по значению (в т.ч. defensive-copy от <c>in</c>) адрес сломается.</remarks>
		public PtrOffset GetNodeOffset(NodeId nodeId, DataLayout scope)
		{
			return nodeLayoutOffsets.Get(nodeId)[scope];
		}

		/// <summary>Абсолютный адрес слайса ноды в static-блоке (static-область живёт в этой же арене).</summary>
		/// <remarks>См. <see cref="GetNodeOffset"/>: вызывать только через ref/арена-указатель.</remarks>
		public SafePtr GetStaticNodeSlice(NodeId nodeId)
		{
			var slice = staticBlock + GetNodeOffset(nodeId, DataLayout.Static);
			return allocatorOffset.GetPtr(slice);
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
