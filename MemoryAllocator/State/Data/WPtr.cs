using System.Runtime.InteropServices;
using Sapientia.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// WorldPtr
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct WPtr : System.IEquatable<WPtr>
	{
		public static readonly WPtr Invalid = new (default, default);

		public AllocatorPtr allocatorPtr;
		public WorldId worldId;

		[INLINE(256)]
		public WPtr(AllocatorPtr allocatorPtr, WorldId worldId)
		{
			this.allocatorPtr = allocatorPtr;
			this.worldId = worldId;
		}

		[INLINE(256)]
		public WPtr GetArrayElement(int elementSize, int index)
		{
			return new WPtr(allocatorPtr.GetArrayElement(elementSize, index), worldId);
		}

		[INLINE(256)]
		public readonly bool IsCreated() => allocatorPtr.IsValid();
		[INLINE(256)]
		public bool IsValid() => IsCreated() && worldId.IsValid();

		[INLINE(256)]
		public readonly bool IsZeroSized() => allocatorPtr.IsZeroSized();

		[INLINE(256)]
		public static bool operator ==(in WPtr m1, in WPtr m2)
		{
			return m1.allocatorPtr == m2.allocatorPtr;
		}

		[INLINE(256)]
		public static bool operator !=(in WPtr m1, in WPtr m2)
		{
			return !(m1 == m2);
		}

		[INLINE(256)]
		public bool Equals(WPtr other)
		{
			return allocatorPtr == other.allocatorPtr;
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is WPtr other && Equals(other);
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return allocatorPtr.GetHashCode();
		}

		[INLINE(256)]
		public World GetWorld()
		{
			return worldId.GetWorld();
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			return GetWorld().GetSafePtr(this);
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			world.MemFree(this);
			this = Invalid;
		}

		[INLINE(256)]
		public void Dispose()
		{
			Dispose(GetWorld());
		}

		[INLINE(256)]
		public WPtr CopyTo(World srsWorld, World dstWorld)
		{
			return srsWorld.CopyPtrTo(dstWorld, this);
		}

		[INLINE(256)]
		public override string ToString() => $"{allocatorPtr.ToString()}, {nameof(worldId)}: [{worldId}]";

		[INLINE(256)]
		public static implicit operator AllocatorPtr(WPtr ptr)
		{
			return ptr.allocatorPtr;
		}
	}
}
