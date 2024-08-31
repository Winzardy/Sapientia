using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	public unsafe readonly struct MemBlockOffset
	{
		public readonly long value;

		[INLINE(256)]
		public MemBlockOffset(void* block, MemZone* zone)
		{
			value = (byte*)block - (byte*)zone;
		}

		[INLINE(256)]
		public MemBlock* Ptr(void* zone)
		{
			return (MemBlock*)((byte*)zone + value);
		}

		[INLINE(256)]
		public static bool operator ==(MemBlockOffset a, MemBlockOffset b) => a.value == b.value;

		[INLINE(256)]
		public static bool operator !=(MemBlockOffset a, MemBlockOffset b) => a.value != b.value;

		[INLINE(256)]
		public bool Equals(MemBlockOffset other)
		{
			return value == other.value;
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is MemBlockOffset other && Equals(other);
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return value.GetHashCode();
		}
	}
}
