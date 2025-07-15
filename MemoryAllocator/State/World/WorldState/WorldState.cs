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
		private bool _checkNullRef;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _worldStateData.IsValid && _worldStateData.Value(false).version > 0;
		}

		private ref WorldStateData WorldStateData
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _worldStateData.Value(_checkNullRef);
		}

		public ref WorldId WorldId
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref WorldStateData.worldId;
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
			_worldStateData = MemoryExt.NullableMemAlloc<WorldStateData>();
			_worldStateData.Value() = new WorldStateData(worldId, initialSize);
			_checkNullRef = true;
		}

		private void EnableInnerChecks()
		{
			_checkNullRef = true;
		}

		private void DisableInnerChecks()
		{
			_checkNullRef = false;
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
		private readonly ref UnsafeServiceRegistry GetLocalServiceRegistry()
		{
			return ref WorldStateData.localServiceRegistry;
		}

		public static WorldState Deserialize(ref StreamBufferReader stream)
		{
			var worldState = new WorldState();

			worldState._worldStateData = MemoryExt.NullableMemAlloc<WorldStateData>();
			worldState.WorldStateData = WorldStateData.Deserialize(ref stream);

			return worldState;
		}

		public void SetupNewWorldId(WorldId newWorldId)
		{
			E.ASSERT(IsValid);

			WorldStateData.SetupNewWorldId(newWorldId);
		}

		public void Reset()
		{
			E.ASSERT(IsValid);

			WorldStateData.Reset();
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public UpdateScope GetUpdateScope()
		{
			return new UpdateScope(ref this);
		}

		public readonly ref struct UpdateScope
		{
			private readonly SafePtr<WorldState> _worldState;

			public UpdateScope(ref WorldState worldState)
			{
				_worldState = worldState.AsSafePtr();
				_worldState.Value().DisableInnerChecks();
			}

			public void Dispose()
			{
				_worldState.Value().EnableInnerChecks();
			}
		}
	}
}
