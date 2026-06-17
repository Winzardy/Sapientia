using System;
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
	/// <see cref="CompiledBlueprintStorage"/>). <b>Чистая рантайм-структура: о authoring-<see cref="Blueprint"/>
	/// (нодах/портах/связях) ничего не знает</b> — её строит <see cref="BlueprintCompiler"/> (единственное место,
	/// знающее об authoring-стороне), а здесь только поля блоба + рантайм-аксессоры. Состав:
	/// <list type="bullet">
	/// <item><b>Data</b> — на ноду (<see cref="NodeHeader"/>): индекс метода + прямая ссылка на static-слайс
	/// (<see cref="NodeHeader.staticData"/>, без общего static-массива) + офсеты слайсов в Cache/InstancePersistence.</item>
	/// <item><b>Map</b> — на ноду блок In/Out (массив <see cref="RegionPtr"/>, In'ы затем Out'ы), на который
	/// указывает <see cref="NodeHeader.inOut"/> (офсет от позиции заголовка). Заполняется только на компиляции;
	/// в run'е читает нода.</item>
	/// <item><b>Map (топология)</b> — <see cref="nodesMap"/>: граф связей нод для шедулинга (предшественники/
	/// потомки + корни).</item>
	/// <item><b>ContextType</b> — <see cref="contextTypes"/>: дедуп-union типов ambient-контекста
	/// (<c>TypeId&lt;INodeContext&gt;</c>), нужных нодам; реестр-владелец наполняет <c>ExecutionScope</c> (4F).</item>
	/// </list>
	/// Static-данные адресуются self-relative (<see cref="RelativePtr{T}"/>/<see cref="BumpArray{T}"/>), отдельный
	/// указатель на арену не нужен. Cache/InstancePersistence только размечаются (размеры/офсеты, <see cref="blockSizes"/>) —
	/// их память заводит владелец Runtime-данных.
	/// </summary>
	public struct CompiledBlueprintHeader
	{
		// Ключ версии блюпринта (id + version). version ≈ generation для staleness (см. VersionedId).
		// Id<Blueprint> — только тег идентичности (phantom-тип), не поведенческая связь с authoring-Blueprint.
		public VersionedId<Blueprint> blueprintKey;

		/// <summary>Полный размер блока каждого региона (для Static — суммарные байты слайсов+констант; для
		/// InstancePersistence — размер блока, который аллоцирует владелец Runtime-памяти).</summary>
		public DataSizes blockSizes;

		/// <summary>Раскладка Cache инстанса (<c>InstanceCache</c> — два раздельных блока): число ячеек метаданных
		/// (<see cref="CacheLink"/>) и суммарный размер блока значений в байтах. Считает <see cref="BlueprintCompiler"/>.</summary>
		public int cacheCellCount;
		public int cacheValuesSize;

		/// <summary>На ноду (по <see cref="Id{NodeHeader}"/>): индекс метода, ссылка на static-слайс,
		/// офсеты Cache/InstancePersistence, указатель на блок In/Out. Self-relative.</summary>
		public BumpArray<NodeHeader> nodes;

		/// <summary>Топология связей нод (граф зависимостей) для шедулинга. Заполняет <see cref="BlueprintCompiler"/>.</summary>
		public NodeMapHeader nodesMap;

		/// <summary>Дедуплицированный union типов ambient-контекста (<c>TypeId&lt;INodeContext&gt;</c>), нужных нодам
		/// блюпринта (4E). Инстанс-агностичен (дедуп вместе с блобом); сортирован по id (детерминизм). На
		/// скомпилированной ноде не хранится; реестр-владелец «тип → указатель» — <c>ExecutionScope</c> (4F). Self-relative.</summary>
		public BumpArray<TypeId<INodeContext>> contextTypes;

		/// <summary>Шаблон Cache-ячеек инстанса (<b>индекс = ordinal</b> = <see cref="RegionPtr.cacheData"/> Cache-порта):
		/// каждая ячейка с забейканным <c>valueOffset</c> (офсет значения в <c>_values</c>) и <c>state = Uninitialized</c>.
		/// <c>InstanceCache</c> создаёт/сбрасывает свой <c>_cells</c> простым копированием этого массива (Reset = copy).
		/// Self-relative; пусто, если Cache-Out'ов нет.</summary>
		public BumpArray<CacheLink> cacheCellsTemplate;

		public readonly int NodesCount => nodes.Length;

		// ─────────────────────────── Доступ ───────────────────────────

		public readonly int GetBlockSize(MemoryRegion region)
		{
			return blockSizes[region];
		}

		/// <summary>Заголовок ноды (<see cref="NodeHeader"/>) по id — единая точка доступа: читай нужные поля
		/// (<see cref="NodeHeader.typeId"/> — ordinal диспатча M6-C, <see cref="NodeHeader.runtimeType"/> — бэкенд M6-D)
		/// через него, а не дёргай массив на каждое поле. <c>readonly ref</c> — без копии и без мутации.</summary>
		/// <remarks>Поля-значения (typeId/runtimeType/persistence/state) читать прямо. Self-relative
		/// <see cref="NodeHeader.staticData"/>/<see cref="NodeHeader.inOut"/> резолвить через
		/// <see cref="GetStaticNodeSlice"/>/<see cref="GetNodeInOut"/> (на копии readonly-ref-поля адрес сломается).
		/// Метод <b>не</b> <c>readonly</c>: внутри self-relative <see cref="nodes"/> (<see cref="BumpArray{T}"/>) — на
		/// defensive-копии заголовка адрес бы сломался (как у прочих аксессоров блоба).</remarks>
		public ref readonly NodeHeader GetNode(Id<NodeHeader> nodeId)
		{
			return ref nodes.Get(nodeId);
		}

		/// <summary>Офсет слайса ноды в блоке InstancePersistence (Static — <see cref="GetStaticNodeSlice"/>;
		/// Cache — per-instance через CacheLink, не хранится на заголовке).</summary>
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

		/// <summary>Топология ноды: ноды-предшественники/потомки (<see cref="NodeRelativesHeader"/>).</summary>
		/// <remarks>Вызывать только через ref/арена-указатель: внутри self-relative <see cref="BumpArray{T}"/>
		/// — на копии по значению (в т.ч. defensive-copy от <c>in</c>) адрес сломается.</remarks>
		public ref NodeRelativesHeader GetNodeRelatives(Id<NodeHeader> nodeId)
		{
			return ref nodesMap.relatives.Get(nodeId);
		}

		/// <summary>Число нод-предшественников ноды (счётчик зависимостей для батч-шедулинга, 4B).</summary>
		public int GetNodeInDegree(Id<NodeHeader> nodeId)
		{
			return nodesMap.relatives.Get(nodeId).InDegree;
		}

		/// <summary>Число корней исполнения (нод без предшественников).</summary>
		public readonly int StartNodeCount => nodesMap.startNodes.Length;

		/// <summary>i-й корень исполнения (нода с <c>inDegree == 0</c>).</summary>
		/// <remarks>Через ref/арена-указатель: self-relative <see cref="BumpArray{T}"/>.</remarks>
		public Id<NodeHeader> GetStartNode(int index)
		{
			return nodesMap.startNodes.Get(index);
		}

		/// <summary>Все типы ambient-контекста (<c>TypeId&lt;INodeContext&gt;</c>) одним span'ом — дедуп-union,
		/// сортированный по id (пригоден под бинарный поиск тип→локальный индекс). Пусто → пустой span.</summary>
		/// <remarks>Вызывать только через ref/арена-указатель: self-relative <see cref="BumpArray{T}"/>; span
		/// валиден, пока блоб жив и не перемещён (на копии по значению адрес сломается).</remarks>
		public ReadOnlySpan<TypeId<INodeContext>> GetContextTypes()
		{
			return contextTypes.GetSpan();
		}

		/// <summary>Указатель на шаблон Cache-ячеек (<see cref="cacheCellsTemplate"/>) для копирования в <c>InstanceCache</c>
		/// при создании/сбросе. Пусто (<see cref="cacheCellCount"/> == 0) → <c>default</c>.</summary>
		/// <remarks>Вызывать только через ref/арена-указатель: self-relative <see cref="BumpArray{T}"/>; блоб стабилен
		/// (off-allocator), но указатель не пережить move/serialize — копировать сразу.</remarks>
		public SafePtr<CacheLink> GetCacheCellsTemplate()
		{
			return cacheCellCount > 0 ? cacheCellsTemplate.GetPtr() : default;
		}
	}
}
