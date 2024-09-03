using System.Runtime.InteropServices;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct MemPtr : System.IEquatable<MemPtr>
	{
		public static readonly MemPtr Invalid = new (0, 0, default);

		public int zoneId;
		public int offset;
		public AllocatorId allocatorId;

		[INLINE(256)]
		public MemPtr(int zoneId, int offset, AllocatorId allocatorId)
		{
			this.zoneId = zoneId;
			this.offset = offset;
			this.allocatorId = allocatorId;
		}

		[INLINE(256)]
		public readonly bool IsValid() => offset > 0u;

		[INLINE(256)]
		public static bool operator ==(in MemPtr m1, in MemPtr m2)
		{
			return m1.zoneId == m2.zoneId && m1.offset == m2.offset;
		}

		[INLINE(256)]
		public static bool operator !=(in MemPtr m1, in MemPtr m2)
		{
			return !(m1 == m2);
		}

		[INLINE(256)]
		public bool Equals(MemPtr other)
		{
			return zoneId == other.zoneId && offset == other.offset;
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is MemPtr other && Equals(other);
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return System.HashCode.Combine(zoneId, offset);
		}

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return allocatorId.GetAllocatorPtr();
		}

		[INLINE(256)]
		public void* GetPtr()
		{
			return GetAllocatorPtr()->GetUnsafePtr(in this);
		}

		public override string ToString() => $"zoneId: {zoneId}, offset: {offset}, allocatorId: [{allocatorId}]";
	}
}
