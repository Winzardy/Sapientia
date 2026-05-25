using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Memory;
using Sapientia.MemoryAllocator.State;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState : IEquatable<WorldState>
	{
		private SafePtr<WorldStateData> _worldStateData;
		private WorldId _worldId;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _worldStateData.IsValid && _worldId.IsValid() && _worldStateData.Value().version > 0;
		}

		internal ref WorldStateData WorldStateData
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				E.ASSERT(_worldId.IsValid(), "WorldId не валиден, вероятно WorldState не существует.");
				return ref _worldStateData.Value();
			}
		}

		public WorldId WorldId
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _worldId;
		}

		public ushort Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => WorldStateData.version;
		}

		public ref uint Tick
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref WorldStateData.tick;
		}

		public ref float Time
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref WorldStateData.time;
		}

		public WorldState(WorldId worldId, int initialSize)
		{
			_worldStateData = MemoryExt.MemAlloc<WorldStateData>();
			_worldStateData.Value() = new WorldStateData(initialSize);
			_worldId = worldId;
			// Registries не инициализируем здесь — WorldManager.CreateWorld после конструктора зовёт
			// SetupNewWorldId с обновлённой версией worldId. Если бы MemArray создавался сейчас,
			// его внутренний _worldId был бы со старой версией → AssertWorldState mismatch на первом Get.
		}

		/// <summary>
		/// Allocates <see cref="MemArray{T}"/> backing storage for in-state registries.
		/// MemArray сохраняет внутри текущий <see cref="WorldId"/> — поэтому init **должен** вызываться
		/// после финального <see cref="SetupNewWorldId"/>, иначе assertion на первом Get.
		/// </summary>
		private void InitializeInStateRegistries()
		{
			ref var data = ref WorldStateData;
			data.serviceRegistry = new IndexedRegistry<IWorldService, IndexedPtr>(this);
			data.componentsManager = new IndexedRegistry<IComponent, CachedPtr<ComponentSet>>(this);
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			WorldStateData.Dispose();
			MemoryExt.MemFree(_worldStateData);

			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal readonly ref Allocator GetAllocator()
		{
			return ref WorldStateData.allocator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal readonly ref IndexedRegistry<IWorldService, IndexedPtr> GetServiceRegistry()
		{
			return ref WorldStateData.serviceRegistry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal readonly ref UnsafeIndexedRegistry<IWorldLocalUnmanagedService, SafePtr> GetNoStateServiceRegistry()
		{
			return ref WorldStateData.noStateServiceRegistry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal readonly ref IndexedRegistry<IComponent, CachedPtr<ComponentSet>> GetComponentsManager()
		{
			return ref WorldStateData.componentsManager;
		}

		public static WorldState Deserialize(ref StreamBufferReader stream)
		{
			var worldState = new WorldState();

			worldState._worldStateData = MemoryExt.MemAlloc<WorldStateData>();
			worldState.WorldStateData = WorldStateData.Deserialize(ref stream);
			// Caller ОБЯЗАН вызвать SetupNewWorldId(worldId) после Deserialize — это единая точка init
			// in-state registries и установки _worldId. Если snapshot был сделан до первого Register —
			// registries придут как default, SetupNewWorldId их пересоздаст с актуальным worldId.

			return worldState;
		}

		public void SetupNewWorldId(WorldId newWorldId)
		{
			_worldId = newWorldId;
			// In-state registries создаются здесь, не в ctor: MemArray._worldId должен совпадать с finalized worldId.
			InitializeInStateRegistries();
		}

		public void Reset()
		{
			E.ASSERT(IsValid);

			WorldStateData.Reset();
			// In-state registries не инициализируются здесь — WorldManager.RemoveWorld после Reset
			// зовёт SetupNewWorldId, который и есть единая точка init. Если бы init был здесь —
			// MemArray создавался бы дважды подряд (Reset+SetupNewWorldId), второй перетирал бы первый
			// без Dispose (allocator pop работает, но invariant "один init на жизненный цикл" нарушается).
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(WorldState left, WorldState right)
		{
			return left._worldStateData == right._worldStateData;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(WorldState left, WorldState right)
		{
			return left._worldStateData != right._worldStateData;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(WorldState other)
		{
			return _worldStateData == other._worldStateData;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is WorldState other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return _worldStateData.GetHashCode();
		}
	}
}
