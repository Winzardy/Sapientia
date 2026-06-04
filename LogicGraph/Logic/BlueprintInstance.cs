using Sapientia.Data;
using Sapientia.MemoryAllocator;
using Submodules.Sapientia.Data;
using Submodules.Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	public struct BlueprintInstance
	{
		public int version;
		public Id<Blueprint> blueprintId;
		public Id<BlueprintInstance> instanceId;

		// Долгоживущие данные инстанса (instance persistent) — не обнуляются между run'ами.
		public CachedPtr instancePersistent;
		// Кеш инстанса (instance cache) — обнуляется каждый run (ResetCache).
		public CachedPtr instanceCache;

		public static CachedPtr<BlueprintInstance> Create(WorldState worldState, in CompiledBlueprint compiledBlueprint, Id<BlueprintInstance> instanceId)
		{
			var resultPtr = CachedPtr<BlueprintInstance>.Create(worldState);

			// Аллоцируем блоки до получения ref на инстанс — чтобы не держать ref через аллокации.
			var instancePersistent = AllocBlock(worldState, compiledBlueprint.GetBlockSize(DataLayout.InstancePersistent));
			var instanceCache = AllocBlock(worldState, compiledBlueprint.GetBlockSize(DataLayout.InstanceCache));

			ref var result = ref resultPtr.GetValue(worldState);
			result.version = compiledBlueprint.version;
			result.blueprintId = compiledBlueprint.id;
			result.instanceId = instanceId;
			result.instancePersistent = instancePersistent;
			result.instanceCache = instanceCache;

			return resultPtr;
		}

		/// <summary>
		/// Аллоцирует обнулённый блок области в worldState. Нулевой размер -> невалидный <see cref="CachedPtr"/>
		/// (нода/блюпринт не занимает эту область — zero-size раскладывается чисто, без выделения).
		/// </summary>
		private static CachedPtr AllocBlock(WorldState worldState, int size)
		{
			if (size <= 0)
				return CachedPtr.Invalid;

			var memPtr = worldState.MemAlloc(size, out var ptr);
			// Основной Allocator не гарантирует обнуление — чистим под детерминизм reset/persistent.
			MemoryExt.MemClear(ptr, size);
			return new CachedPtr(worldState, ptr, memPtr);
		}

		/// <summary>Обнуляет блок instance cache (вызывается перед каждым run'ом). Persistent не трогает.</summary>
		public void ResetCache(WorldState worldState, in CompiledBlueprint compiledBlueprint)
		{
			if (!instanceCache.IsValid())
				return;

			var size = compiledBlueprint.GetBlockSize(DataLayout.InstanceCache);
			MemoryExt.MemClear(instanceCache.GetPtr(worldState), size);
		}

		public void Dispose(WorldState worldState)
		{
			if (instanceCache.IsValid())
				instanceCache.Dispose(worldState);
			if (instancePersistent.IsValid())
				instancePersistent.Dispose(worldState);
		}
	}
}
