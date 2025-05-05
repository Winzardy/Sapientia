using System.Runtime.InteropServices;
using Sapientia.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public struct MemPtr : System.IEquatable<MemPtr>
	{
		public static readonly MemPtr Invalid = new (0, 0);

		public int zoneId;
		public int zoneOffset;

		[INLINE(256)]
		public MemPtr(int zoneId, int zoneOffset)
		{
			this.zoneId = zoneId;
			this.zoneOffset = zoneOffset;
		}

		[INLINE(256)]
		public static MemPtr CreateZeroSized()
		{
			return new MemPtr(0, -1);
		}

		[INLINE(256)]
		public MemPtr GetArrayElement(int elementSize, int index)
		{
			return new MemPtr(zoneId, zoneOffset + index * elementSize);
		}

		[INLINE(256)]
		public readonly bool IsValid() => zoneOffset != 0;
		[INLINE(256)]
		public readonly bool IsZeroSized() => zoneOffset < 0;

		[INLINE(256)]
		public static bool operator ==(in MemPtr m1, in MemPtr m2)
		{
			return m1.zoneId == m2.zoneId && m1.zoneOffset == m2.zoneOffset;
		}

		[INLINE(256)]
		public static bool operator !=(in MemPtr m1, in MemPtr m2)
		{
			return !(m1 == m2);
		}

		[INLINE(256)]
		public bool Equals(MemPtr other)
		{
			return zoneId == other.zoneId && zoneOffset == other.zoneOffset;
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is MemPtr other && Equals(other);
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return System.HashCode.Combine(zoneId, zoneOffset);
		}

		[INLINE(256)]
		public SafePtr GetPtr(ref Allocator allocator)
		{
			return allocator.GetSafePtr(this);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			allocator.MemFree(this);
			this = Invalid;
		}

		[INLINE(256)]
		public MemPtr CopyTo(ref Allocator srsAllocator, ref Allocator dstAllocator)
		{
			return srsAllocator.CopyPtrTo(ref dstAllocator, this);
		}

		[INLINE(256)]
		public override string ToString() => $"{nameof(zoneId)}: {zoneId}, {nameof(zoneOffset)}: {zoneOffset}";
	}
}
