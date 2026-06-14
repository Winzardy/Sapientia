using System;
using System.Collections.Generic;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Заголовок <b>Static</b> блюпринта: read-only данные, существующие в единственном экземпляре на
	/// приложение (дедуп по <see cref="blueprintKey"/>; живут в собственной bump-арене, см.
	/// <see cref="CompiledBlueprintStorage"/>). Состав:
	/// <list type="bullet">
	/// <item><b>Data</b> — на ноду (<see cref="NodeHeader"/>): индекс метода + прямая ссылка на static-слайс
	/// (<see cref="NodeHeader.staticData"/>, без общего static-массива) + офсеты слайсов в Cache/Persistence.</item>
	/// <item><b>Map</b> — на ноду блок In/Out (массив <see cref="RegionPtr"/>, In'ы затем Out'ы), на который
	/// указывает <see cref="NodeHeader.inOut"/> (офсет от позиции заголовка). Заполняется только на компиляции;
	/// в run'е читает нода. Регион Out выводится из типа порта (precalculated → Static, persistent →
	/// Persistence, иначе Cache); In указывает на данные источника.</item>
	/// <item>ContextType — следующий инкремент.</item>
	/// </list>
	/// Static-данные адресуются self-relative (<see cref="RelativePtr{T}"/>/<see cref="BumpArray{T}"/>), отдельный
	/// указатель на арену не нужен. Cache/Persistence только размечаются (размеры/офсеты, <see cref="blockSizes"/>) —
	/// их память заводит владелец Runtime-данных. Компиляция — managed-путь (editor/server), не Burst.
	/// </summary>
	public struct CompiledBlueprintHeader
	{
		// Ключ версии блюпринта (id + version). version ≈ generation для staleness (см. VersionedId).
		public VersionedId<Blueprint> blueprintKey;

		/// <summary>Полный размер блока каждого региона (для Static — суммарные байты слайсов+констант; для
		/// Cache/Persistence — размер блока, который аллоцирует владелец Runtime-памяти).</summary>
		public DataSizes blockSizes;

		/// <summary>На ноду (по <see cref="Id{NodeHeader}"/>): индекс метода, ссылка на static-слайс,
		/// офсеты Cache/Persistence, указатель на блок In/Out. Self-relative.</summary>
		public BumpArray<NodeHeader> nodes;

		public NodeMapHeader nodesMap;

		public readonly int NodesCount => nodes.Length;

		/// <summary>
		/// Сколько байт зарезервировать в static-арене. Держится в lockstep с <see cref="SetupLayout"/>
		/// по нумерованным шагам (1..4) — менять только вместе.
		/// </summary>
		public static int CalculateLayoutSizeToReserve(Blueprint blueprint)
		{
			var size = TSize<CompiledBlueprintHeader>.size; // 1. сама структура

			var nodeCount = blueprint.nodes.Length;
			if (nodeCount == 0)
				return size;

			size += TSize<NodeHeader>.size * nodeCount; // 2. заголовки нод (Data)

			var staticBytes = 0;
			foreach (var node in blueprint.nodes)
				staticBytes += node.DataSizes.GetAligned(MemoryRegion.Static);
			staticBytes += CalculateConstantsSize(blueprint);
			size += staticBytes; // 3. static-слайсы нод + константы (каждый аллоцируется отдельно)

			size += TSize<RegionPtr>.size * CountPorts(blueprint); // 4. блоки In/Out нод (RegionPtr на порт)

			return size;
		}

		/// <summary>
		/// Standalone-компиляция одного блюпринта в собственную raw-арену.
		/// Владение ареной — на вызывающем (Dispose).
		/// </summary>
		public static RawBumpAllocator CompileLayout(Blueprint blueprint, out PtrOffset<CompiledBlueprintHeader> offset)
		{
			var reservedSize = CalculateLayoutSizeToReserve(blueprint);
			var arena = new RawBumpAllocator(reservedSize);

			ref var allocator = ref arena.Value;
			ref var compiled = ref allocator.MemAlloc(out offset); // 1. структура
			compiled.SetupLayout(ref allocator, blueprint);

			return arena;
		}

		private void SetupLayout(ref BumpHeader allocator, Blueprint blueprint)
		{
			// Ключ static-блоба — (id, version).
			blueprintKey = new VersionedId<Blueprint>(blueprint.id, blueprint.version);

			var bpNodes = blueprint.nodes;
			var nodeCount = bpNodes.Length;
			if (nodeCount == 0)
				return;

			nodes.Alloc(ref allocator, nodeCount); // 2. заголовки нод (in-place, self-relative)

			// Data: индекс метода + офсеты Cache/Persistence (сумма выровненных слайсов) + отдельный static-слайс.
			for (var i = 0; i < nodeCount; i++)
			{
				ref var header = ref nodes.Get(i);
				header.typeId = bpNodes[i].NodeTypeId;
				// TODO(runtimeType): заполнять из ноды (нужен INode.RuntimeType) — для оркестратора (M7), пока default.
				// Cache-офсет ноды на заголовке не храним (Cache уходит в DataCache, per-instance) — считаем
				// при сборке Map локально (см. SetupMap). Persistence остаётся per-node офсетом блока.
				header.persistence = new PtrOffset(blockSizes[MemoryRegion.Persistence]);

				// Static-слайс ноды — отдельная аллокация; прямая self-relative ссылка в заголовке.
				var staticSize = bpNodes[i].DataSizes.GetAligned(MemoryRegion.Static);
				if (staticSize > 0)
				{
					var staticOffset = allocator.MemAlloc(staticSize); // 3a. static-слайс ноды
					header.staticData.SetPtr(allocator.GetPtr(staticOffset).Cast<byte>());
				}

				blockSizes += bpNodes[i].DataSizes.GetAligned();
			}

			// 4. Блоки In/Out нод заполняет SetupMap.
			SetupMap(ref allocator, blueprint);
		}

		/// <summary>
		/// Строит блоки In/Out нод (массив <see cref="RegionPtr"/> на ноду, <see cref="NodeHeader.inOut"/>) по связям
		/// блюпринта: размещает Out'ы нод (Static — в их static-слайсах с
		/// бейком дефолта; Cache/Persistence — офсетом в блоке региона), константы — отдельными аллокациями (с
		/// бейком), затем для каждого In записывает указатель его источника (<see cref="Blueprint.inputToOutput"/>).
		/// </summary>
		private void SetupMap(ref BumpHeader allocator, Blueprint blueprint)
		{
			var bpNodes = blueprint.nodes;
			// Managed-структуры допустимы: путь компиляции — editor/server-side, не Burst и не per-frame.
			// Цель Out: для Static — адрес в блобе; для Cache/Persistence — (регион, офсет в блоке).
			var outTarget = new Dictionary<NodeOutput, OutTarget>();

			// Pass 1: Out'ы каждой ноды.
			Span<int> localRover = stackalloc int[DataSizes.Count];
			// Cache-офсет слайса ноды (на заголовке не хранится — Cache уходит в DataCache): сумма выровненных
			// Cache-слайсов предыдущих нод. Двигаем для КАЖДОЙ ноды (в т.ч. без портов), порядок == SetupLayout.
			var cacheNodeOffset = 0;
			for (var n = 0; n < bpNodes.Length; n++)
			{
				var outputs = bpNodes[n].GetOutputs();
				if (outputs != null && outputs.Length > 0)
				{
					localRover.Clear();
					var declaredSizes = bpNodes[n].DataSizes;
					ref var header = ref nodes.Get(n);
					foreach (var output in outputs)
					{
						var region = GetOutputRegion(output);
						var regionIndex = region.ToInt();

						E.ASSERT(localRover[regionIndex] + output.DataSize <= declaredSizes[region],
							"[CompiledBlueprintHeader] Out'ы ноды не помещаются в её слайс региона (DataSizes меньше суммы Out'ов).");
						E.ASSERT(!outTarget.ContainsKey(output), "[CompiledBlueprintHeader] Один Out принадлежит двум нодам.");

						var intra = localRover[regionIndex];
						localRover[regionIndex] += output.DataSize.AlignUp(DataSizes.Alignment);

						if (region == MemoryRegion.Static)
						{
							// Адрес ячейки Out внутри static-слайса ноды; бейкаем дефолт прямо в блоб.
							var target = (SafePtr)header.staticData.GetPtr() + intra;
							output.SetValue(target);
							outTarget[output] = OutTarget.Static(target);
						}
						else
						{
							// Cache/Persistence: офсет слайса ноды в блоке региона + позиция Out внутри слайса.
							var nodeRegionOffset = region == MemoryRegion.Cache ? cacheNodeOffset : header.persistence.byteOffset;
							outTarget[output] = OutTarget.Runtime(region, nodeRegionOffset + intra);
						}
					}
				}

				cacheNodeOffset += bpNodes[n].DataSizes.GetAligned(MemoryRegion.Cache);
			}

			// Pass 2: константы (precalculated-Out без ноды-владельца) — отдельной аллокацией, с бейком дефолта.
			if (blueprint.outputs != null)
			{
				foreach (var output in blueprint.outputs)
				{
					if (output == null || !output.IsPreCalculated || outTarget.ContainsKey(output))
						continue;

					var constSize = output.DataSize.AlignUp(DataSizes.Alignment);
					var constOffset = allocator.MemAlloc(constSize); // 3b. константа
					var target = allocator.GetPtr(constOffset);
					output.SetValue(target);
					outTarget[output] = OutTarget.Static(target);

					// Константы — тоже Static-байты (slices + constants); held lockstep с CalculateConstantsSize.
					blockSizes[MemoryRegion.Static] += constSize;
				}
			}

			// Pass 3: на ноду — блок In/Out (массив RegionPtr: In'ы, затем Out'ы), офсет от позиции заголовка.
			var headerPtr = (SafePtr)this.AsSafePtr();
			for (var n = 0; n < bpNodes.Length; n++)
			{
				var inputs = bpNodes[n].GetInputs();
				var outputs = bpNodes[n].GetOutputs();
				var inCount = inputs?.Length ?? 0;
				var outCount = outputs?.Length ?? 0;
				var portCount = inCount + outCount;

				ref var header = ref nodes.Get(n);
				if (portCount == 0)
				{
					header.inOut = default;
					continue;
				}

				var blockOffset = allocator.MemAlloc(portCount * TSize<RegionPtr>.size); // 4. блок In/Out ноды
				var blockPtr = allocator.GetPtr(blockOffset);
				header.inOut = blockPtr - headerPtr; // PtrOffset от позиции CompiledBlueprintHeader
				var slots = blockPtr.Cast<RegionPtr>().GetSpan(portCount);

				for (var i = 0; i < inCount; i++)
				{
					// Каждый вход обязан иметь источник: дефолтное значение — тоже Out (константа).
					OutTarget target = default;
					var hasSource = blueprint.inputToOutput != null
						&& blueprint.inputToOutput.TryGetValue(inputs[i], out var source)
						&& outTarget.TryGetValue(source, out target);
					E.ASSERT(hasSource, "[CompiledBlueprintHeader] In не связан ни с одним Out (inputToOutput).");

					WriteSlot(ref slots[i], target);
				}
				for (var j = 0; j < outCount; j++)
					WriteSlot(ref slots[inCount + j], outTarget[outputs[j]]);
			}
		}

		/// <summary>Пишет указатель Map в слот: Static — self-relative (SetPtr на месте), иначе — офсет в блоке.</summary>
		private static void WriteSlot(ref RegionPtr slot, OutTarget target)
		{
			slot.region = target.region;
			if (target.region == MemoryRegion.Static)
				slot.data.SetPtr(target.staticPtr.Cast<byte>()); // self-relative от адреса слота
			else
				slot.data = new RelativePtr<byte>(target.runtimeOffset); // .byteOffset = офсет в блоке региона
		}

		/// <summary>Регион, куда пишет Out: константа → Static (RO), persistent-стейт → Persistence, иначе Cache.</summary>
		private static MemoryRegion GetOutputRegion(NodeOutput output)
		{
			if (output.IsPreCalculated)
				return MemoryRegion.Static;
			if (output.IsPersistent)
				return MemoryRegion.Persistence;
			return MemoryRegion.Cache;
		}

		private static int CalculateConstantsSize(Blueprint blueprint)
		{
			if (blueprint.outputs == null)
				return 0;

			// Симметрично SetupMap: Out, принадлежащий ноде, размещается в её слайсе и в константы не идёт;
			// повтор одного экземпляра в outputs считается один раз.
			HashSet<NodeOutput> placed = null;
			foreach (var node in blueprint.nodes)
			{
				var outputs = node.GetOutputs();
				if (outputs == null || outputs.Length == 0)
					continue;
				placed ??= new HashSet<NodeOutput>();
				foreach (var output in outputs)
					placed.Add(output);
			}

			var size = 0;
			foreach (var output in blueprint.outputs)
			{
				if (output == null || !output.IsPreCalculated)
					continue;
				placed ??= new HashSet<NodeOutput>();
				if (!placed.Add(output))
					continue;
				size += output.DataSize.AlignUp(DataSizes.Alignment);
			}
			return size;
		}

		private static int CountPorts(Blueprint blueprint)
		{
			var count = 0;
			foreach (var node in blueprint.nodes)
			{
				var inputs = node.GetInputs();
				var outputs = node.GetOutputs();
				count += (inputs?.Length ?? 0) + (outputs?.Length ?? 0);
			}
			return count;
		}

		// ─────────────────────────── Доступ ───────────────────────────

		public readonly int GetBlockSize(MemoryRegion region)
		{
			return blockSizes[region];
		}

		/// <summary>Индекс метода обработки ноды (Static.Data).</summary>
		public TypeId<INode> GetNodeTypeId(Id<NodeHeader> nodeId)
		{
			return nodes.Get(nodeId).typeId;
		}

		/// <summary>Офсет слайса ноды в блоке Persistence (Static — <see cref="GetStaticNodeSlice"/>;
		/// Cache — per-instance через DataCache, не хранится на заголовке).</summary>
		public PtrOffset GetNodePersistenceOffset(Id<NodeHeader> nodeId)
		{
			return nodes.Get(nodeId).persistence;
		}

		/// <summary>Прямой адрес static-слайса ноды в блобе (self-relative).</summary>
		/// <remarks>Вызывать только через ref/арена-указатель: self-relative <see cref="NodeHeader.staticData"/>
		/// на копии по значению (в т.ч. defensive-copy от <c>in</c>) сломает адрес.</remarks>
		public SafePtr GetStaticNodeSlice(Id<NodeHeader> nodeId)
		{
			return (SafePtr)nodes.Get(nodeId).staticData.GetPtr();
		}

		/// <summary>
		/// Блок In/Out ноды (массив <see cref="RegionPtr"/>: In'ы, затем Out'ы) — резолв
		/// <c>адрес заголовка + офсет</c> (<see cref="NodeHeader.inOut"/>). Число портов знает сама нода;
		/// невалидный (нет портов) → <c>default</c>. Static-указатели внутри резолвятся «на месте» (через ref).
		/// </summary>
		/// <remarks>Вызывать только через ref/арена-указатель: офсет идёт от <c>&amp;this</c>; на копии по
		/// значению (в т.ч. defensive-copy от <c>in</c>) адрес сломается.</remarks>
		public unsafe SafePtr GetNodeInOut(Id<NodeHeader> nodeId)
		{
			var inOut = nodes.Get(nodeId).inOut;
			if (!inOut.isValid)
				return default;

			var headerPtr = (SafePtr)this.AsSafePtr();
			return new SafePtr(headerPtr.ptr + inOut.byteOffset);
		}

		/// <summary>Цель Out на этапе компиляции: Static-адрес в блобе либо (регион, офсет) Runtime-памяти.</summary>
		private struct OutTarget
		{
			public MemoryRegion region;
			public SafePtr staticPtr;     // для Static
			public int runtimeOffset;     // для Cache/Persistence

			public static OutTarget Static(SafePtr ptr)
			{
				return new OutTarget { region = MemoryRegion.Static, staticPtr = ptr };
			}

			public static OutTarget Runtime(MemoryRegion region, int offset)
			{
				return new OutTarget { region = region, runtimeOffset = offset };
			}
		}
	}
}
