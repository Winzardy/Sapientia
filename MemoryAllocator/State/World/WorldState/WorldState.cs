using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Memory;
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

		private ref WorldStateData WorldStateData
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
		private readonly ref Allocator GetAllocator()
		{
			return ref WorldStateData.allocator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly ref ServiceRegistry GetServiceRegistry()
		{
			return ref WorldStateData.serviceRegistry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private readonly ref UnsafeServiceRegistry GetNoStateServiceRegistry()
		{
			return ref WorldStateData.noStateServiceRegistry;
		}

		public static WorldState Deserialize(ref StreamBufferReader stream)
		{
			var worldState = new WorldState();

			worldState._worldStateData = MemoryExt.MemAlloc<WorldStateData>();
			worldState.WorldStateData = WorldStateData.Deserialize(ref stream);

			return worldState;
		}

		public void SetupNewWorldId(WorldId newWorldId)
		{
			_worldId = newWorldId;
		}

		public void Reset()
		{
			E.ASSERT(IsValid);

			WorldStateData.Reset();
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
