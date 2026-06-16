using System;
using Sapientia.Collections;
using Sapientia.Data;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Коннектор Static↔Runtime: домен исполнения, владелец per-instance памяти + трекер живых инстансов.
	/// Память инстанса — <b>два независимых блока</b> (по природе данных), каждый — самостоятельная обёртка над
	/// фиксированной off-allocator-ареной:
	/// <list type="bullet">
	/// <item><b><see cref="InstanceCache"/></b> (<see cref="_cache"/>) — ячейки <see cref="CacheLink{T}"/>; <b>транзиентный,
	/// сброс каждый run, НЕ часть стейта</b> (в снапшот/слой State не идёт) ⇒ лежит <b>отдельно</b> от стейта.</item>
	/// <item><b><see cref="InstancePersistence"/></b> (<see cref="_memory"/>) — постоянный per-instance стейт (часть состояния
	/// домена). Динамику нода берёт через контекст (напр. <c>WorldState</c> + <c>MemArray</c> — 4F-2).</item>
	/// </list>
	/// Размеры блоков — из Static-блоба (<see cref="CompiledBlueprintHeader.GetBlockSize"/>), который <b>читается</b>
	/// из <see cref="CompiledBlueprintStorage"/> (передаётся в <see cref="CreateInstance"/>, не хранится — развязка
	/// ответственности). Identity + generation-staleness — на <see cref="BlueprintInstanceStorage"/>.
	/// </summary>
	/// <remarks>Runtime off-allocator, транзиентно. <see cref="InstanceCache"/>/<see cref="InstancePersistence"/> позиционно-независимы
	/// (резолв через хендл своей арены) — потому хранятся прямо в списках по id. Ambient-context-реестр
	/// («тип контекста → указатель», по <see cref="CompiledBlueprintHeader.GetContextTypes"/>) — на scope, общий для всех
	/// инстансов домена (<see cref="_contexts"/>, 4F-2): реестр сам владеет памятью контекстов.</remarks>
	public struct ExecutionScope : IDisposable
	{
		private Id<MemoryManager> _memoryId;
		private BlueprintInstanceStorage _instances;  // identity + generation-staleness
		private UnsafeList<InstanceCache> _cache;              // транзиентный кеш по id (ОТДЕЛЬНО от стейта)
		private UnsafeList<InstancePersistence> _memory;       // стейт (InstancePersistence) по id
		private ContextRegistry<INodeContext> _contexts;       // ambient-context на scope (тип контекста → указатель)

		public readonly bool IsCreated => _instances.IsCreated;
		public readonly int InstanceCount => _instances.Count;

		public static ExecutionScope Create(Id<MemoryManager> memoryId = default, int instanceCapacity = 8)
		{
			var cap = instanceCapacity > 0 ? instanceCapacity : 1;
			return new ExecutionScope
			{
				_memoryId = memoryId,
				_instances = new BlueprintInstanceStorage(cap),
				_cache = new UnsafeList<InstanceCache>(memoryId, cap),
				_memory = new UnsafeList<InstancePersistence>(memoryId, cap),
				_contexts = ContextRegistry<INodeContext>.Create(memoryId),
			};
		}

		/// <summary>
		/// Создаёт инстанс блюпринта <paramref name="bp"/>: заводит ему <see cref="InstanceCache"/> и <see cref="InstancePersistence"/>
		/// по размерам блоба, трекает в сторедже. Static-блоб читается из <paramref name="storage"/> (не сохраняется).
		/// Возвращает хендл инстанса.
		/// </summary>
		public BlueprintInstanceId CreateInstance(ref CompiledBlueprintStorage storage, VersionedId<Blueprint> bp)
		{
			E.ASSERT(storage.Has(bp), "[ExecutionScope] CreateInstance на отсутствующем в storage блюпринте.");
			ref var compiled = ref storage.Get(bp);

			var cache = InstanceCache.Create(_memoryId, compiled.cacheCellCount, compiled.cacheValuesSize, compiled.GetCacheCellsTemplate());
			var persistence = InstancePersistence.Create(_memoryId, compiled.GetBlockSize(MemoryRegion.Persistence));

			// BlueprintInstanceHeader — identity-сущность; память владеет scope (Cache/InstancePersistence), офсеты не нужны.
			var header = BlueprintInstanceHeader.Create(in compiled, default, default);
			var id = _instances.Add(header);

			_cache.EnsureCount(id.id + 1, default);
			_cache[id.id] = cache;
			_memory.EnsureCount(id.id + 1, default);
			_memory[id.id] = persistence;

			return id;
		}

		/// <summary>Жив ли хендл (слот занят и generation совпадает — не stale).</summary>
		public readonly bool Has(BlueprintInstanceId id)
		{
			return _instances.Has(id);
		}

		/// <summary><see cref="InstanceCache"/> инстанса (Read/Write/Reset кеша портов). Вызывать на живом хендле.</summary>
		public ref InstanceCache GetInstanceCache(BlueprintInstanceId id)
		{
			E.ASSERT(_instances.Has(id), "[ExecutionScope] GetInstanceCache на мёртвом/stale хендле.");
			return ref _cache[id.id];
		}

		/// <summary><see cref="InstancePersistence"/> инстанса (постоянный per-instance стейт). Вызывать на живом хендле.</summary>
		public ref InstancePersistence GetInstancePersistent(BlueprintInstanceId id)
		{
			E.ASSERT(_instances.Has(id), "[ExecutionScope] GetInstancePersistent на мёртвом/stale хендле.");
			return ref _memory[id.id];
		}

		/// <summary>Сброс кеша инстанса перед run'ом (все ячейки → <see cref="CacheState.Uninitialized"/>).</summary>
		public void ResetInstanceCache(BlueprintInstanceId id)
		{
			GetInstanceCache(id).Reset();
		}

		/// <summary>Сброс кеша у всех живых инстансов домена (старт run'а). Пустые/невалидные слоты — no-op.</summary>
		public void ResetAllCache()
		{
			for (var i = 0; i < _cache.count; i++)
				_cache[i].Reset();
		}

		// ─────────────────────────── Ambient context (на scope) ───────────────────────────

		/// <summary>Кладёт ambient-контекст <typeparamref name="T"/> по значению (реестр выделяет память и копирует).</summary>
		public void SetContext<T>(in T value) where T : unmanaged, INodeContext
		{
			_contexts.SetContext(value);
		}

		/// <summary><c>readonly ref</c> на ambient-контекст <typeparamref name="T"/> (read-only; задать — <see cref="SetContext{T}"/>). Резолв нодой в run'е — M7.</summary>
		public readonly ref readonly T GetContext<T>() where T : unmanaged, INodeContext
		{
			return ref _contexts.GetContext<T>();
		}

		/// <summary>Задан ли ambient-контекст <typeparamref name="T"/>.</summary>
		public readonly bool HasContext<T>() where T : INodeContext
		{
			return _contexts.HasContext<T>();
		}

		/// <summary>Уничтожает инстанс: освобождает его <see cref="InstanceCache"/> и <see cref="InstancePersistence"/> и снимает с трека. No-op для stale/неизвестного.</summary>
		public void DisposeInstance(BlueprintInstanceId id)
		{
			if (!_instances.Has(id))
				return;

			ref var cache = ref _cache[id.id];
			cache.Dispose();
			cache = default;

			ref var persistence = ref _memory[id.id];
			persistence.Dispose();
			persistence = default;

			_instances.Remove(id);
		}

		public void Dispose()
		{
			if (!IsCreated)
				return;

			for (var i = 0; i < _cache.count; i++)
				_cache[i].Dispose();
			for (var i = 0; i < _memory.count; i++)
				_memory[i].Dispose();

			_cache.Dispose();
			_memory.Dispose();
			_instances.Dispose();
			_contexts.Dispose();
			this = default;
		}
	}
}
