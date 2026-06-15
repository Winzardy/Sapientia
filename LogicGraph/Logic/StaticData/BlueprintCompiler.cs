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
	/// Компилятор блюпринта: authoring-<see cref="Blueprint"/> (ноды + связи + дефолты) → <b>Static-блоб</b>
	/// (<see cref="CompiledBlueprintHeader"/>) в собственной raw-арене. <b>Единственное место, знающее об
	/// authoring-стороне</b> (<see cref="INode"/>, порты <see cref="NodeInput"/>/<see cref="NodeOutput"/>,
	/// <see cref="Blueprint.inputToOutput"/>): сам <see cref="CompiledBlueprintHeader"/> о <see cref="Blueprint"/>
	/// ничего не знает (чистая рантайм-структура). Раскладывает Data (заголовки нод + static-слайсы), Map (In/Out
	/// → <see cref="RegionPtr"/>), константы и топологию (<see cref="NodeMapHeader"/>); держит lockstep
	/// <see cref="CalculateLayoutSizeToReserve"/> ⟷ <see cref="SetupLayout"/>. Managed-путь (editor/server), не Burst.
	/// </summary>
	public static class BlueprintCompiler
	{
		/// <summary>
		/// Сколько байт зарезервировать в static-арене. Держится в lockstep с <see cref="SetupLayout"/>
		/// по нумерованным шагам (1..5) — менять только вместе.
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

			// 5. nodeMap: массив relatives + рёбра (inputs/outputs нод) + startNodes.
			var adjacency = BuildAdjacency(blueprint);
			size += TSize<NodeRelativesHeader>.size * nodeCount; // 5a. массив relatives (по ноде)
			for (var i = 0; i < nodeCount; i++)
				size += (adjacency.preds[i].Length + adjacency.succs[i].Length) * TSize<Id<NodeHeader>>.size; // 5b. рёбра
			size += adjacency.startNodes.Length * TSize<Id<NodeHeader>>.size; // 5c. корни

			// 7. contextTypes: дедуп-union типов ambient-контекста (шаг 6 — флаги NodeState — памяти не занимает).
			size += TSize<TypeId<INodeContext>>.size * BuildContextTypes(blueprint).Length;

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
			SetupLayout(ref compiled, ref allocator, blueprint);

			return arena;
		}

		private static void SetupLayout(ref CompiledBlueprintHeader compiled, ref BumpHeader allocator, Blueprint blueprint)
		{
			// Ключ static-блоба — (id, version).
			compiled.blueprintKey = new VersionedId<Blueprint>(blueprint.id, blueprint.version);

			var bpNodes = blueprint.nodes;
			var nodeCount = bpNodes.Length;
			if (nodeCount == 0)
				return;

			compiled.nodes.Alloc(ref allocator, nodeCount); // 2. заголовки нод (in-place, self-relative)

			// Data: индекс метода + офсеты Cache/Persistence (сумма выровненных слайсов) + отдельный static-слайс.
			for (var i = 0; i < nodeCount; i++)
			{
				ref var header = ref compiled.nodes.Get(i);
				header.typeId = bpNodes[i].NodeTypeId;
				header.runtimeType = bpNodes[i].RuntimeType; // форк 8: бэкенд исполнения (бакетинг батчей — M7)
				// Cache-офсет ноды на заголовке не храним (Cache уходит в DataCache, per-instance) — считаем
				// при сборке Map локально (см. SetupMap). Persistence остаётся per-node офсетом блока.
				header.persistence = new PtrOffset(compiled.blockSizes[MemoryRegion.Persistence]);

				// Static-слайс ноды — отдельная аллокация; прямая self-relative ссылка в заголовке.
				var staticSize = bpNodes[i].DataSizes.GetAligned(MemoryRegion.Static);
				if (staticSize > 0)
				{
					var staticOffset = allocator.MemAlloc(staticSize); // 3a. static-слайс ноды
					header.staticData.SetPtr(allocator.GetPtr(staticOffset).Cast<byte>());
				}

				compiled.blockSizes += bpNodes[i].DataSizes.GetAligned();
			}

			// 4. Блоки In/Out нод заполняет SetupMap.
			SetupMap(ref compiled, ref allocator, blueprint);

			// 5. Топология (relatives + startNodes) — инстанс-агностична, бейкается в блоб.
			BuildNodeMap(ref compiled, ref allocator, blueprint);

			// 6. Флаги нод (NodeState) — после Map (Multiple читает топологию). Не влияют на sizing (вне lockstep).
			SetupNodeFlags(ref compiled, blueprint);

			// 7. Типы ambient-контекста (дедуп-union по нодам) — для ExecutionScope (4F). На ноде не хранятся.
			SetupContextTypes(ref compiled, ref allocator, blueprint);
		}

		/// <summary>
		/// Строит блоки In/Out нод (массив <see cref="RegionPtr"/> на ноду, <see cref="NodeHeader.inOut"/>) по связям
		/// блюпринта: размещает Out'ы нод (Static — в их static-слайсах с
		/// бейком дефолта; Cache/Persistence — офсетом в блоке региона), константы — отдельными аллокациями (с
		/// бейком), затем для каждого In записывает указатель его источника (<see cref="Blueprint.inputToOutput"/>).
		/// </summary>
		private static void SetupMap(ref CompiledBlueprintHeader compiled, ref BumpHeader allocator, Blueprint blueprint)
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
					ref var header = ref compiled.nodes.Get(n);
					foreach (var output in outputs)
					{
						var region = GetOutputRegion(output);
						var regionIndex = region.ToInt();
						// Cache-Out занимает ячейку DataCache (тег+payload, форк 1); Static/Persistence — сырой размер.
						var slotSize = region == MemoryRegion.Cache ? output.CacheCellSize : output.DataSize;

						E.ASSERT(localRover[regionIndex] + slotSize <= declaredSizes[region],
							"[BlueprintCompiler] Out'ы ноды не помещаются в её слайс региона (DataSizes меньше суммы Out'ов/ячеек).");
						E.ASSERT(!outTarget.ContainsKey(output), "[BlueprintCompiler] Один Out принадлежит двум нодам.");

						var intra = localRover[regionIndex];
						localRover[regionIndex] += slotSize.AlignUp(DataSizes.Alignment);

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
					compiled.blockSizes[MemoryRegion.Static] += constSize;
				}
			}

			// Pass 3: на ноду — блок In/Out (массив RegionPtr: In'ы, затем Out'ы), офсет от позиции заголовка.
			var headerPtr = (SafePtr)compiled.AsSafePtr();
			for (var n = 0; n < bpNodes.Length; n++)
			{
				var inputs = bpNodes[n].GetInputs();
				var outputs = bpNodes[n].GetOutputs();
				var inCount = inputs?.Length ?? 0;
				var outCount = outputs?.Length ?? 0;
				var portCount = inCount + outCount;

				ref var header = ref compiled.nodes.Get(n);
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
					E.ASSERT(hasSource, "[BlueprintCompiler] In не связан ни с одним Out (inputToOutput).");

					WriteSlot(ref slots[i], target);
				}
				for (var j = 0; j < outCount; j++)
					WriteSlot(ref slots[inCount + j], outTarget[outputs[j]]);
			}
		}

		/// <summary>
		/// Бейкает топологию нод (<see cref="NodeMapHeader"/>) в блоб: per-node ноды-предшественники/потомки
		/// (<see cref="NodeRelativesHeader"/>) + корни (<see cref="NodeMapHeader.startNodes"/>). Считает то же
		/// разбиение, что <see cref="BuildAdjacency"/> в <see cref="CalculateLayoutSizeToReserve"/> (lockstep).
		/// </summary>
		private static void BuildNodeMap(ref CompiledBlueprintHeader compiled, ref BumpHeader allocator, Blueprint blueprint)
		{
			var adjacency = BuildAdjacency(blueprint);
			var nodeCount = blueprint.nodes.Length;

			// Массив relatives — in-place в блобе (self-relative); затем per-node inputs/outputs.
			compiled.nodesMap.relatives.Alloc(ref allocator, nodeCount);
			for (var i = 0; i < nodeCount; i++)
			{
				// ref в арену: inner-BumpArray'и считают self-relative офсет от своего финального адреса.
				ref var relatives = ref compiled.nodesMap.relatives.Get(i);

				var preds = adjacency.preds[i];
				relatives.inputs.Alloc(ref allocator, preds.Length);
				for (var k = 0; k < preds.Length; k++)
					relatives.inputs.Get(k) = (Id<NodeHeader>)preds[k];

				var succs = adjacency.succs[i];
				relatives.outputs.Alloc(ref allocator, succs.Length);
				for (var k = 0; k < succs.Length; k++)
					relatives.outputs.Get(k) = (Id<NodeHeader>)succs[k];
			}

			var roots = adjacency.startNodes;
			compiled.nodesMap.startNodes.Alloc(ref allocator, roots.Length);
			for (var k = 0; k < roots.Length; k++)
				compiled.nodesMap.startNodes.Get(k) = (Id<NodeHeader>)roots[k];
		}

		/// <summary>
		/// Выставляет флаги <see cref="NodeState"/> на ноду (форк 7): <see cref="NodeState.HasCache"/> — есть хотя бы
		/// один Out в Cache-регионе (мемоизация Is-Calculated, M8); <see cref="NodeState.Multiple"/> — fan-out, Out'ы
		/// ноды читают ≥2 потребителя (<c>relatives.outputs.Length > 1</c>). Топология уже забейкана
		/// (<see cref="BuildNodeMap"/>); флаги на sizing не влияют (поле в уже аллоцированном <see cref="NodeHeader"/>).
		/// </summary>
		private static void SetupNodeFlags(ref CompiledBlueprintHeader compiled, Blueprint blueprint)
		{
			var bpNodes = blueprint.nodes;
			for (var i = 0; i < bpNodes.Length; i++)
			{
				var state = new ByteEnumMask<NodeState>();

				if (HasCacheOut(bpNodes[i]))
					state.Add(NodeState.HasCache);

				// Multiple — fan-out по топологии: relatives.outputs — дедуп нод-потребителей (Length безопасен на копии).
				if (compiled.GetNodeRelatives(i).outputs.Length > 1)
					state.Add(NodeState.Multiple);

				ref var header = ref compiled.nodes.Get(i);
				header.state = state;
			}
		}

		/// <summary>True, если у ноды есть хотя бы один Out, попадающий в Cache-регион (тот же критерий региона,
		/// что и в <see cref="SetupMap"/> — <see cref="GetOutputRegion"/>).</summary>
		private static bool HasCacheOut(INode node)
		{
			var outputs = node.GetOutputs();
			if (outputs == null)
				return false;
			foreach (var output in outputs)
			{
				if (output != null && GetOutputRegion(output) == MemoryRegion.Cache)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Бейкает в блоб дедуп-union типов ambient-контекста (<see cref="CompiledBlueprintHeader.contextTypes"/>):
		/// собирает <c>TypeId&lt;INodeContext&gt;</c> со всех нод (см. <see cref="BuildContextTypes"/>) — то же разбиение,
		/// что в <see cref="CalculateLayoutSizeToReserve"/> (lockstep). Пустой union — аллокации нет (<c>contextTypes</c>
		/// остаётся <c>default</c>, <see cref="CompiledBlueprintHeader.GetContextTypes"/> возвращает пустой span).
		/// </summary>
		private static void SetupContextTypes(ref CompiledBlueprintHeader compiled, ref BumpHeader allocator, Blueprint blueprint)
		{
			var contextTypes = BuildContextTypes(blueprint);
			if (contextTypes.Length == 0)
				return;

			compiled.contextTypes.Alloc(ref allocator, contextTypes.Length);
			for (var i = 0; i < contextTypes.Length; i++)
				compiled.contextTypes.Get(i) = contextTypes[i];
		}

		/// <summary>
		/// Собирает дедуплицированный, сортированный по id union типов ambient-контекста (<c>TypeId&lt;INodeContext&gt;</c>)
		/// со всех нод блюпринта (<see cref="INode.GetContextTypes"/>). Дедуп — по id (через <see cref="HashSet{T}"/>),
		/// порядок — по возрастанию id (детерминизм). <b>Единый источник</b> разбиения для sizing (шаг 7) и заполнения
		/// (<see cref="SetupContextTypes"/>) — lockstep.
		/// </summary>
		private static TypeId<INodeContext>[] BuildContextTypes(Blueprint blueprint)
		{
			HashSet<int> ids = null;
			foreach (var node in blueprint.nodes)
			{
				var types = node.GetContextTypes();
				if (types == null)
					continue;
				foreach (var type in types)
				{
					ids ??= new HashSet<int>();
					ids.Add(type); // TypeId<INodeContext> → int (id)
				}
			}

			if (ids == null)
				return Array.Empty<TypeId<INodeContext>>();

			var sorted = new int[ids.Count];
			ids.CopyTo(sorted);
			Array.Sort(sorted);

			var result = new TypeId<INodeContext>[sorted.Length];
			for (var i = 0; i < sorted.Length; i++)
				result[i] = sorted[i]; // int → TypeId<INodeContext>
			return result;
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

		/// <summary>
		/// Строит нод-граф зависимостей из связей блюпринта (<see cref="Blueprint.inputToOutput"/>) + портов.
		/// Рёбра — <b>по нодам</b> (дедуп): несколько In'ов из одной ноды-источника дают одно ребро. «Готовый»
		/// источник (precalculated-Out, забейкан в Static, в т.ч. принадлежащий ноде; либо Out без ноды-владельца)
		/// ребра не создаёт. Порядок соседей — по возрастанию индекса ноды (детерминизм). <b>Единый источник</b>
		/// разбиения для sizing (шаг 5) и заполнения (<see cref="BuildNodeMap"/>) — lockstep.
		/// </summary>
		private static Adjacency BuildAdjacency(Blueprint blueprint)
		{
			var bpNodes = blueprint.nodes;
			var nodeCount = bpNodes.Length;

			// NodeOutput -> индекс ноды-владельца (из GetOutputs каждой ноды).
			var owner = new Dictionary<NodeOutput, int>();
			for (var i = 0; i < nodeCount; i++)
			{
				var outputs = bpNodes[i].GetOutputs();
				if (outputs == null)
					continue;
				foreach (var output in outputs)
				{
					if (output != null)
						owner[output] = i;
				}
			}

			var predSets = new HashSet<int>[nodeCount];
			var succSets = new HashSet<int>[nodeCount];
			for (var i = 0; i < nodeCount; i++)
			{
				predSets[i] = new HashSet<int>();
				succSets[i] = new HashSet<int>();
			}

			for (var i = 0; i < nodeCount; i++)
			{
				var inputs = bpNodes[i].GetInputs();
				if (inputs == null || blueprint.inputToOutput == null)
					continue;
				foreach (var input in inputs)
				{
					if (input == null)
						continue;
					if (!blueprint.inputToOutput.TryGetValue(input, out var source) || source == null)
						continue;
					// Precalculated-Out (константа, забейкана в Static): готов без исполнения owner'а — ребра нет.
					// Единый критерий «готового» источника с SetupMap (IsPreCalculated → Static).
					if (source.IsPreCalculated)
						continue;
					// Источник без ноды-владельца (висячий): зависимости нет.
					if (!owner.TryGetValue(source, out var ownerNode))
						continue;
					if (ownerNode == i)
						continue; // самопетля — игнор
					predSets[i].Add(ownerNode);
					succSets[ownerNode].Add(i);
				}
			}

			var preds = new int[nodeCount][];
			var succs = new int[nodeCount][];
			var startNodes = new List<int>();
			for (var i = 0; i < nodeCount; i++)
			{
				preds[i] = ToSortedArray(predSets[i]);
				succs[i] = ToSortedArray(succSets[i]);
				if (preds[i].Length == 0)
					startNodes.Add(i);
			}

			return new Adjacency(preds, succs, startNodes.ToArray());
		}

		private static int[] ToSortedArray(HashSet<int> set)
		{
			var array = new int[set.Count];
			set.CopyTo(array);
			Array.Sort(array);
			return array;
		}

		/// <summary>Нод-граф зависимостей (managed-промежуточная форма): на ноду предшественники/потомки + корни.</summary>
		private readonly struct Adjacency
		{
			public readonly int[][] preds;    // на ноду: ноды-предшественники (sorted, дедуп)
			public readonly int[][] succs;    // на ноду: ноды-потомки (sorted, дедуп)
			public readonly int[] startNodes; // ноды с inDegree == 0 (по возрастанию)

			public Adjacency(int[][] preds, int[][] succs, int[] startNodes)
			{
				this.preds = preds;
				this.succs = succs;
				this.startNodes = startNodes;
			}
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
