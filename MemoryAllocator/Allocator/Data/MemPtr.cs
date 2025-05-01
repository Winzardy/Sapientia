using System.Runtime.InteropServices;
using Sapientia.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MemPtr : System.IEquatable<MemPtr>
	{
		public static readonly MemPtr Invalid = new (0, 0, default);

		public int zoneId;
		public int zoneOffset;
		public AllocatorId allocatorId;

		[INLINE(256)]
		public MemPtr(int zoneId, int zoneOffset, AllocatorId allocatorId)
		{
			this.zoneId = zoneId;
			this.zoneOffset = zoneOffset;
			this.allocatorId = allocatorId;
		}

		public static MemPtr CreateZeroSized(AllocatorId allocatorId)
		{
			return new MemPtr(0, -1, allocatorId);
		}

		public MemPtr GetArrayElement(int elementSize, int index)
		{
			return new MemPtr(zoneId, zoneOffset + index * elementSize, allocatorId);
		}

		[INLINE(256)]
		public readonly bool IsCreated() => zoneOffset != 0;
		[INLINE(256)]
		public bool IsValid() => IsCreated() && allocatorId.IsValid();
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
		public Allocator GetAllocator()
		{
			return allocatorId.GetAllocator();
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			return GetAllocator().GetSafePtr(in this);
		}

		[INLINE(256)]
		public void Dispose(Allocator allocator)
		{
			allocator.MemFree(this);
			this = Invalid;
		}

		[INLINE(256)]
		public void Dispose()
		{
			GetAllocator().MemFree(this);
			this = Invalid;
		}

		public MemPtr CopyTo(Allocator srsAllocator, Allocator dstAllocator)
		{
			return srsAllocator.CopyPtrTo(dstAllocator, this);
		}

		public override string ToString() => $"zoneId: {zoneId}, offset: {zoneOffset}, allocatorId: [{allocatorId}]";
	}
}
