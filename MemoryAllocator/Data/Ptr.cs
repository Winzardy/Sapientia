using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Data
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Ptr
	{
		public static readonly Ptr Invalid = new (MemPtr.Invalid);

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ushort _version;
		private SafePtr _cachedPtr;
		public MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsCreated() => memPtr.IsCreated();
		[INLINE(256)]
		public bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public Ptr(MemPtr memPtr) : this(0, default, memPtr) {}

		[INLINE(256)]
		public Ptr(ushort version, SafePtr cachedPtr, MemPtr memPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public Ptr(SafePtr<Allocator> allocator, SafePtr cachedPtr, MemPtr memPtr)
		{
			_version = allocator.ptr->version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public bool IsValid(SafePtr<Allocator> allocator)
		{
			return _version == allocator.ptr->version;
		}

		[INLINE(256)]
		public SafePtr<Allocator> GetAllocatorPtr()
		{
			return memPtr.GetAllocatorPtr();
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			var allocator = memPtr.GetAllocatorPtr();
			if (allocator.Value().version != _version)
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr GetPtr(SafePtr<Allocator> allocator)
		{
			if (allocator.Value().version != _version)
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>(SafePtr<Allocator> allocator) where T: unmanaged
		{
			if (allocator.Value().version != _version)
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T Get<T>() where T : unmanaged
		{
			var allocator = memPtr.GetAllocatorPtr();
			if (allocator.Value().version != _version)
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return ref _cachedPtr.Value<T>();
		}

		[INLINE(256)]
		public ref T Get<T>(SafePtr<Allocator> allocator) where T : unmanaged
		{
			if (allocator.Value().version != _version)
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return ref _cachedPtr.Value<T>();
		}

		[INLINE(256)]
		public void Dispose(SafePtr<Allocator> allocator)
		{
			memPtr.Dispose(allocator);
			this = Invalid;
		}

		[INLINE(256)]
		public void Dispose()
		{
			memPtr.Dispose();
			this = Invalid;
		}

		public Ptr CopyTo(SafePtr<Allocator> srsAllocator, SafePtr<Allocator> dstAllocator)
		{
			return new Ptr(memPtr.CopyTo(srsAllocator, dstAllocator));
		}

		[INLINE(256)]
		public static implicit operator Ptr(MemPtr value)
		{
			return new Ptr(value);
		}

		public static bool operator ==(Ptr a, Ptr b)
		{
			return a.memPtr == b.memPtr;
		}

		public static bool operator !=(Ptr a, Ptr b)
		{
			return a.memPtr != b.memPtr;
		}

		public override int GetHashCode()
		{
			return memPtr.GetHashCode();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Ptr<T> : IEquatable<Ptr<T>> where T : unmanaged
	{
		public static readonly Ptr<T> Invalid = new (MemPtr.Invalid);

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ushort _version;
		private SafePtr<T> _cachedPtr;
		public MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsCreated() => memPtr.IsCreated();
		[INLINE(256)]
		public bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public Ptr(MemPtr memPtr) : this(0, default, memPtr) {}

		[INLINE(256)]
		public Ptr(ushort version, SafePtr<T> cachedPtr, MemPtr memPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public Ptr(SafePtr<Allocator> allocator, SafePtr<T> cachedPtr, MemPtr memPtr)
		{
			_version = allocator.Value().version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public Ptr(SafePtr<Allocator> allocator, SafePtr<T> cachedPtr, MemPtr memPtr, in T value)
		{
			_version = allocator.Value().version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;

			cachedPtr[0] = value;
		}

		[INLINE(256)]
		public static Ptr<T> Create(SafePtr<Allocator> allocator)
		{
			var memPtr = allocator.Value().MemAlloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, cachedPtr, memPtr);
		}

		[INLINE(256)]
		public static Ptr<T> Create(SafePtr<Allocator> allocator, in T value)
		{
			var memPtr = allocator.Value().MemAlloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, cachedPtr, memPtr, value);
		}

		[INLINE(256)]
		public static SafePtr<T> Create(SafePtr<Allocator> allocator, out Ptr<T> ptr)
		{
			var memPtr = allocator.Value().MemAlloc<T>(out var cachedPtr);
			ptr = new Ptr<T>(allocator, cachedPtr, memPtr);
			return cachedPtr;
		}

		[INLINE(256)]
		public static Ptr<T> Create()
		{
			var allocator = AllocatorManager.CurrentAllocatorPtr;
			var memPtr = allocator.Value().MemAlloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, cachedPtr, memPtr);
		}

		[INLINE(256)]
		public Ptr<T1> ToCachedPtr<T1>() where T1 : unmanaged
		{
			return new Ptr<T1>(_version, _cachedPtr.Cast<T1>(), memPtr);
		}

		[INLINE(256)]
		public SafePtr<Allocator> GetAllocatorPtr()
		{
			return memPtr.allocatorId.GetAllocatorPtr();
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr()
		{
			var allocator = GetAllocatorPtr();
			if (allocator.Value().version != _version && memPtr.IsCreated())
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr(SafePtr<Allocator> allocator)
		{
			if (allocator.Value().version != _version && memPtr.IsCreated())
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T GetValue()
		{
			var allocator = GetAllocatorPtr();
			if (allocator.Value().version != _version)
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return ref _cachedPtr.Value();
		}

		[INLINE(256)]
		public ref T GetValue(SafePtr<Allocator> allocator)
		{
			if (allocator.Value().version != _version)
			{
				_cachedPtr = allocator.Value().GetSafePtr(in memPtr);
				_version = allocator.Value().version;
			}

			return ref _cachedPtr.Value();
		}

		public Ptr<T> CopyTo(SafePtr<Allocator> srsAllocator, SafePtr<Allocator> dstAllocator)
		{
			return new Ptr<T>(memPtr.CopyTo(srsAllocator, dstAllocator));
		}

		public void Dispose(SafePtr<Allocator> allocator)
		{
			memPtr.Dispose(allocator);
			this = Invalid;
		}

		[INLINE(256)]
		public static implicit operator Ptr(Ptr<T> value)
		{
			return value.As<Ptr<T>, Ptr>();
		}

		[INLINE(256)]
		public static implicit operator Ptr<T>(Ptr value)
		{
			return value.As<Ptr, Ptr<T>>();
		}

		[INLINE(256)]
		public static implicit operator Ptr<T>(MemPtr value)
		{
			return new Ptr<T>(value);
		}

		[INLINE(256)]
		public static implicit operator IndexedPtr(Ptr<T> value)
		{
			return IndexedPtr.Create(value);
		}

		[INLINE(256)]
		public static implicit operator Ptr<T>(IndexedPtr value)
		{
			return value.GetCachedPtr();
		}

		[INLINE(256)]
		public static bool operator ==(Ptr<T> a, Ptr<T> b)
		{
			return a.memPtr == b.memPtr;
		}

		[INLINE(256)]
		public static bool operator !=(Ptr<T> a, Ptr<T> b)
		{
			return a.memPtr != b.memPtr;
		}

		public bool Equals(Ptr<T> other)
		{
			return memPtr == other.memPtr;
		}

		public override int GetHashCode()
		{
			return memPtr.GetHashCode();
		}
	}
}
