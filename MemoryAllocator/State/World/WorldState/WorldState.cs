using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public partial struct WorldState : IEquatable<WorldState>, IDisposable
	{
		private SentinelPtr<WorldStateData> _worldStateData;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _worldStateData.IsValid && _worldStateData.Value().version > 0;
		}

		public ref WorldId WorldId
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _worldStateData.Value().worldId;
		}

		public ushort Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _worldStateData.Value().version;
		}

		public ref uint Tick
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _worldStateData.Value().tick;
		}

		public ref float Time
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _worldStateData.Value().time;
		}

		public WorldState(WorldId worldId, int initialSize)
		{
			_worldStateData = MemoryExt.NullableMemAlloc<WorldStateData>();
			_worldStateData.Value() = new WorldStateData(worldId, initialSize);
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			_worldStateData.Value().Dispose();
			MemoryExt.MemFree(_worldStateData);

			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref Allocator GetAllocator()
		{
			return ref _worldStateData.Value().allocator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref ServiceRegistry GetServiceRegistry()
		{
			return ref _worldStateData.Value().serviceRegistry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref UnsafeServiceRegistry GetLocalServiceRegistry()
		{
			return ref _worldStateData.Value().localServiceRegistry;
		}

		public static WorldState Deserialize(ref StreamBufferReader stream)
		{
			var worldState = new WorldState();

			worldState._worldStateData = MemoryExt.NullableMemAlloc<WorldStateData>();
			worldState._worldStateData.Value() = WorldStateData.Deserialize(ref stream);

			return worldState;
		}

		public void SetupNewWorldId(WorldId newWorldId)
		{
			E.ASSERT(IsValid);

			_worldStateData.Value().SetupNewWorldId(newWorldId);
		}

		public void Reset()
		{
			E.ASSERT(IsValid);

			_worldStateData.Value().Reset();
			// Обновляем ссылку на стейт
			_worldStateData.ResetDisposeSentinel();
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
