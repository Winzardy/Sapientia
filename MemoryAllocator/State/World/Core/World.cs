using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Core;

namespace Sapientia.MemoryAllocator
{
	public partial struct World : IEquatable<World>, IDisposable
	{
		private SafePtr<UnsafeWorld> _unsafeWorld;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _unsafeWorld != default && _unsafeWorld.Value().version > 0;
		}

		public ref WorldId WorldId
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _unsafeWorld.Value().worldId;
		}

		public ushort Version
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _unsafeWorld.Value().version;
		}

		public void Initialize(WorldId worldId, int initialSize)
		{
			E.ASSERT(!IsValid);

			_unsafeWorld = MemoryExt.MemAlloc<UnsafeWorld>();
			_unsafeWorld.Value().worldId = worldId;
			_unsafeWorld.Value().version = 1;

			_unsafeWorld.Value().allocator = new Allocator();
			_unsafeWorld.Value().allocator.Initialize(initialSize);

			_unsafeWorld.Value().serviceRegistry = ServiceRegistry.Create(this);
		}

		public void Dispose()
		{
			if (!IsValid)
				return;

			_unsafeWorld.Value().Dispose();
			MemoryExt.MemFree(_unsafeWorld);

			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref Allocator GetAllocator()
		{
			return ref _unsafeWorld.Value().allocator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref ServiceRegistry GetServiceRegistry()
		{
			return ref _unsafeWorld.Value().serviceRegistry;
		}

		public static World Deserialize(ref StreamBufferReader stream)
		{
			var world = new World();

			world._unsafeWorld = MemoryExt.MemAlloc<UnsafeWorld>();
			world._unsafeWorld.Value() = UnsafeWorld.Deserialize(ref stream);

			return world;
		}

		public void Reset(WorldId worldId)
		{
			E.ASSERT(IsValid);

			_unsafeWorld.Value().Reset(worldId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(World left, World right)
		{
			return left._unsafeWorld == right._unsafeWorld;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(World left, World right)
		{
			return left._unsafeWorld != right._unsafeWorld;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(World other)
		{
			return _unsafeWorld == other._unsafeWorld;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is World other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return _unsafeWorld.GetHashCode();
		}
	}
}
