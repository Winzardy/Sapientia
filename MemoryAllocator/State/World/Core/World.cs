using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public partial struct World : IEquatable<World>, IDisposable
	{
		private NullablePtr<WorldState> _worldState;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _worldState != default && _worldState.Value().version > 0;
		}

		public ref WorldId WorldId
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _worldState.Value().worldId;
		}

		public ushort Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _worldState.Value().version;
		}

		public void Initialize(WorldId worldId, int initialSize)
		{
			E.ASSERT(!IsValid);

			_worldState = MemoryExt.NullableMemAlloc<WorldState>();
			_worldState.Value().Initialize(worldId, initialSize);
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			_worldState.Value().Dispose();
			MemoryExt.MemFree(_worldState);

			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref Allocator GetAllocator()
		{
			return ref _worldState.Value().allocator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref ServiceRegistry GetServiceRegistry()
		{
			return ref _worldState.Value().serviceRegistry;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref UnsafeServiceRegistry GetLocalServiceRegistry()
		{
			return ref _worldState.Value().localServiceRegistry;
		}

		public static World Deserialize(ref StreamBufferReader stream)
		{
			var world = new World();

			world._worldState = MemoryExt.NullableMemAlloc<WorldState>();
			world._worldState.Value() = WorldState.Deserialize(ref stream);

			return world;
		}

		public void Reset(WorldId worldId)
		{
			E.ASSERT(IsValid);

			_worldState.Value().Reset(worldId);
		}

		public void Clear()
		{
			E.ASSERT(IsValid);

			_worldState.Value().Clear();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(World left, World right)
		{
			return left._worldState == right._worldState;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(World left, World right)
		{
			return left._worldState != right._worldState;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(World other)
		{
			return _worldState == other._worldState;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is World other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return _worldState.GetHashCode();
		}
	}
}
