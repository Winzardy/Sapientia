using System;
using System.Runtime.CompilerServices;

namespace Submodules.Sapientia.Safety
{
	public readonly struct DisposeSentinel : IDisposable, IEquatable<DisposeSentinel>
	{
		public readonly int id;
		public readonly int version;
		public readonly int typeId;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DisposeSentinel(int id, int version, int typeId)
		{
			this.id = id;
			this.version = version;
			this.typeId = typeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DisposeSentinel Create<T>()
		{
			return DisposeSentinelManager.AllocateSafetyHandle<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static DisposeSentinel Create()
		{
			return DisposeSentinelManager.AllocateSafetyHandle<DisposeSentinel>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValid()
		{
			return DisposeSentinelManager.CheckSafetyHandle(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (!IsValid())
				return;
			DisposeSentinelManager.ReleaseSafetyHandle(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(DisposeSentinel left, DisposeSentinel right)
		{
			return left.id == right.id && left.version == right.version && left.typeId == right.typeId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(DisposeSentinel left, DisposeSentinel right)
		{
			return !(left == right);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(DisposeSentinel other)
		{
			return this == other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object? obj)
		{
			return obj is DisposeSentinel other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return HashCode.Combine(id, version, typeId);
		}
	}
}
