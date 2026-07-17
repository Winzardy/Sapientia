using Sapientia.Data;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Узловой контекст исполнения: что́ статическая функция ноды (<see cref="ExecuteFn"/>) получает и как
	/// добирается до своей памяти. <b>Burst-совместим</b> — только указатели + <see cref="nodeId"/>, без
	/// managed-ссылок. Несёт три региона инстанса (Static-слайс/Cache/Persistence) + ссылку на блоб (Map/топология).
	/// Тело ноды — данные в static-слайсе (<see cref="Body{T}"/>); логика — <c>static Execute(ref NodeContext)</c>
	/// типа ноды (без виртуальных вызовов). Ambient-context (резолв «тип → указатель») сюда придёт в M7.
	/// </summary>
	/// <remarks>Аксессоры <see cref="StaticSlice"/>/<see cref="InOut"/> резолвят self-relative данные блоба —
	/// идут через <see cref="Compiled"/> (ref в блоб), а не через копию заголовка (на копии адрес сломается).</remarks>
	public struct NodeContext
	{
		/// <summary>Блоб блюпринта (Static-слайсы нод + Map + топология). Резолв полей — через <see cref="Compiled"/> (ref).</summary>
		public SafePtr<CompiledBlueprintHeader> compiled;
		/// <summary>Cache-регион инстанса (мемоизация In/Out; сброс каждый run).</summary>
		public SafePtr<InstanceCache> cache;
		/// <summary>Persistence-регион инстанса (постоянный per-instance стейт; может быть невалидным, если у блюпринта нет Persistence).</summary>
		public SafePtr<InstancePersistence> persistence;
		/// <summary>Адрес исполняемой ноды в блобе.</summary>
		public Id<NodeHeader> nodeId;

		/// <summary>Ref на заголовок блоба (для резолва self-relative Static/Map — вызывать аксессоры на нём, не на копии).</summary>
		public ref CompiledBlueprintHeader Compiled()
		{
			return ref compiled.Value();
		}

		/// <summary>Ref на Cache-регион инстанса (для <c>Read</c>/<c>Write</c> Cache-портов).</summary>
		public ref InstanceCache Cache()
		{
			return ref cache.Value();
		}

		/// <summary>Static-слайс ноды (её тело-данные + precalc-константы); резолв через ref блоба.</summary>
		public SafePtr StaticSlice()
		{
			return Compiled().GetStaticNodeSlice(nodeId);
		}

		/// <summary>Тело ноды как <c>ref T</c> поверх static-слайса (читает статическая <c>Execute</c> ноды).</summary>
		public ref T Body<T>() where T : unmanaged
		{
			return ref StaticSlice().Cast<T>().Value();
		}

		/// <summary>Блок In/Out ноды (массив <see cref="RegionPtr"/>: In'ы, затем Out'ы); резолв через ref блоба.</summary>
		public SafePtr InOut()
		{
			return Compiled().GetNodeInOut(nodeId);
		}

		/// <summary>Persistence-слайс ноды: база региона инстанса + офсет слайса. Невалиден, если у инстанса нет Persistence.</summary>
		public SafePtr PersistenceSlice()
		{
			return persistence.Value().GetPtr() + Compiled().GetNodePersistenceOffset(nodeId).byteOffset;
		}
	}
}
