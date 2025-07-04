using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemPtr : System.IEquatable<MemPtr>
	{
		public static readonly MemPtr Invalid = new (0, 0);

		public int zoneId;
		public int zoneOffset;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr(int zoneId, int zoneOffset)
		{
			this.zoneId = zoneId;
			this.zoneOffset = zoneOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static MemPtr CreateZeroSized()
		{
			return new MemPtr(0, -1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr GetArrayElement(int elementSize, int index)
		{
			return new MemPtr(zoneId, zoneOffset + index * elementSize);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool IsValid() => zoneOffset != 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool IsZeroSized() => zoneOffset < 0;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(in MemPtr m1, in MemPtr m2)
		{
			return m1.zoneId == m2.zoneId && m1.zoneOffset == m2.zoneOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(in MemPtr m1, in MemPtr m2)
		{
			return !(m1 == m2);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(MemPtr other)
		{
			return zoneId == other.zoneId && zoneOffset == other.zoneOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is MemPtr other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return System.HashCode.Combine(zoneId, zoneOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr(ref Allocator allocator)
		{
			return allocator.GetSafePtr(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(ref Allocator allocator)
		{
			allocator.MemFree(this);
			this = Invalid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr CopyTo(ref Allocator srsAllocator, ref Allocator dstAllocator)
		{
			return srsAllocator.CopyPtrTo(ref dstAllocator, this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => $"{nameof(zoneId)}: {zoneId}, {nameof(zoneOffset)}: {zoneOffset}";
	}
}
