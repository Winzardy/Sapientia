using Sapientia.Data;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Per-instance стейт блюпринта — <b>чистая рантайм-сущность</b>. <b>Ничего не знает о памяти и её
	/// источнике</b>: ни <c>WorldState</c>, ни Mem-сущностей (<c>MemPtr</c>/<c>SafePtr</c>/аллокатора). На свои
	/// области ссылается <b>абстрактными офсетами</b> (<see cref="PtrOffset"/>) внутри store'ов, которыми владеет
	/// <see cref="ExecutionScope"/>; владелец даёт базу любого источника и резолвит <c>база + офсет</c>.
	///
	/// <list type="bullet">
	/// <item><see cref="instanceCache"/> — офсет слайса в cache-store (off-allocator), обнуляется каждый run.</item>
	/// <item><see cref="instancePersistent"/> — офсет слайса в persistent-store (в снапшоте), не обнуляется.</item>
	/// </list>
	/// Невалидный <see cref="PtrOffset"/> (<c>default</c>) = область не занята (zero-size раскладывается чисто).
	/// </summary>
	public struct BlueprintInstanceHeader
	{
		// Ключ версии блюпринта (id + version). version ≈ generation для staleness (см. VersionedId).
		public VersionedId<Blueprint> blueprintId;

		// Офсеты слайсов в per-site store'ах владельца (ExecutionScope). База — у владельца.
		public PtrOffset instanceCache;
		public PtrOffset instancePersistent;

		/// <summary>
		/// Собирает инстанс из identity скомпилированного блюпринта и <b>офсетов</b> уже зарезервированных
		/// снаружи слайсов. <see cref="instanceId"/> присваивает сторедж при добавлении
		/// (<see cref="BlueprintInstanceStorage.Add"/>).
		/// </summary>
		public static BlueprintInstanceHeader Create(in CompiledBlueprintHeader compiledBlueprint, PtrOffset instanceCache, PtrOffset instancePersistent)
		{
			return new BlueprintInstanceHeader
			{
				blueprintId = compiledBlueprint.blueprintKey,
				instanceCache = instanceCache,
				instancePersistent = instancePersistent,
			};
		}
	}
}
