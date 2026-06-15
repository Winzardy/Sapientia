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
	/// (<see cref="NodeHeader.staticData"/>, без общего static-массива) + офсеты слайсов в Cache/Persistence.</item>
	/// <item><b>Map</b> — на ноду блок In/Out (массив <see cref="RegionPtr"/>, In'ы затем Out'ы), на который
	/// указывает <see cref="NodeHeader.inOut"/> (офсет от позиции заголовка). Заполняется только на компиляции;
	/// в run'е читает нода.</item>
	/// <item><b>Map (топология)</b> — <see cref="nodesMap"/>: граф связей нод для шедулинга (предшественники/
	/// потомки + корни).</item>
	/// <item><b>ContextType</b> — <see cref="contextTypes"/>: дедуп-union типов ambient-контекста
	/// (<c>TypeId&lt;INodeContext&gt;</c>), нужных нодам; реестр-владелец наполняет <c>ExecutionScope</c> (4F).</item>
	/// </list>
	/// Static-данные адресуются self-relative (<see cref="RelativePtr{T}"/>/<see cref="BumpArray{T}"/>), отдельный
	/// указатель на арену не нужен. Cache/Persistence только размечаются (размеры/офсеты, <see cref="blockSizes"/>) —
	/// их память заводит владелец Runtime-данных.
	/// </summary>
	public struct CompiledBlueprintHeader
	{
		// Ключ версии блюпринта (id + version). version ≈ generation для staleness (см. VersionedId).
		// Id<Blueprint> — только тег идентичности (phantom-тип), не поведенческая связь с authoring-Blueprint.
		public VersionedId<Blueprint> blueprintKey;

		/// <summary>Полный размер блока каждого региона (для Static — суммарные байты слайсов+констант; для
		/// Cache/Persistence — размер блока, который аллоцирует владелец Runtime-памяти).</summary>
		public DataSizes blockSizes;

		/// <summary>На ноду (по <see cref="Id{NodeHeader}"/>): индекс метода, ссылка на static-слайс,
		/// офсеты Cache/Persistence, указатель на блок In/Out. Self-relative.</summary>
		public BumpArray<NodeHeader> nodes;

		/// <summary>Топология связей нод (граф зависимостей) для шедулинга. Заполняет <see cref="BlueprintCompiler"/>.</summary>
		public NodeMapHeader nodesMap;

		/// <summary>Дедуплицированный union типов ambient-контекста (<c>TypeId&lt;INodeContext&gt;</c>), нужных нодам
		/// блюпринта (4E). Инстанс-агностичен (дедуп вместе с блобом); сортирован по id (детерминизм). На
		/// скомпилированной ноде не хранится; реестр-владелец «тип → указатель» — <c>ExecutionScope</c> (4F). Self-relative.</summary>
		public BumpArray<TypeId<INodeContext>> contextTypes;

		public readonly int NodesCount => nodes.Length;

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
	}
}
