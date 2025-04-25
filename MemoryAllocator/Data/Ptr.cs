using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Data
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Ptr
	{
		public static readonly Ptr Invalid = new (MemPtr.Invalid);

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
		public Ptr(Allocator allocator, SafePtr cachedPtr, MemPtr memPtr)
		{
			_version = allocator.version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public bool IsValid(Allocator allocator)
		{
			return _version == allocator.version;
		}

		[INLINE(256)]
		public Allocator GetAllocator()
		{
			return memPtr.GetAllocator();
		}

		[INLINE(256)]
		public SafePtr GetPtr()
		{
			var allocator = memPtr.GetAllocator();
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr GetPtr(Allocator allocator)
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>(Allocator allocator) where T: unmanaged
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T Get<T>() where T : unmanaged
		{
			var allocator = memPtr.GetAllocator();
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref _cachedPtr.Value<T>();
		}

		[INLINE(256)]
		public ref T Get<T>(Allocator allocator) where T : unmanaged
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref _cachedPtr.Value<T>();
		}

		[INLINE(256)]
		public void Dispose(Allocator allocator)
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

		public Ptr CopyTo(Allocator srsAllocator, Allocator dstAllocator)
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
		public Ptr(MemPtr memPtr) : this(0, memPtr, default) {}

		[INLINE(256)]
		public Ptr(ushort version, MemPtr memPtr, SafePtr<T> cachedPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public Ptr(Allocator allocator, MemPtr memPtr, SafePtr<T> cachedPtr)
		{
			_version = allocator.version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public Ptr(Allocator allocator, MemPtr memPtr, SafePtr<T> cachedPtr, in T value)
		{
			_version = allocator.version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;

			cachedPtr[0] = value;
		}

		[INLINE(256)]
		public static Ptr<T> Create(Allocator allocator)
		{
			var memPtr = allocator.MemAlloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, memPtr, cachedPtr);
		}

		[INLINE(256)]
		public static Ptr<T> Create(Allocator allocator, in T value)
		{
			var memPtr = allocator.MemAlloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, memPtr, cachedPtr, value);
		}

		[INLINE(256)]
		public static SafePtr<T> Create(Allocator allocator, out Ptr<T> ptr)
		{
			var memPtr = allocator.MemAlloc<T>(out var cachedPtr);
			ptr = new Ptr<T>(allocator, memPtr, cachedPtr);
			return cachedPtr;
		}

		[INLINE(256)]
		public static Ptr<T> Create()
		{
			var allocator = AllocatorManager.CurrentAllocator;
			var memPtr = allocator.MemAlloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, memPtr, cachedPtr);
		}

		[INLINE(256)]
		public Ptr<T1> ToCachedPtr<T1>() where T1 : unmanaged
		{
			return new Ptr<T1>(_version, memPtr, _cachedPtr.Cast<T1>());
		}

		[INLINE(256)]
		public Allocator GetAllocator()
		{
			return memPtr.allocatorId.GetAllocator();
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr()
		{
			var allocator = GetAllocator();
			if (allocator.version != _version && memPtr.IsCreated())
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr(Allocator allocator)
		{
			if (allocator.version != _version && memPtr.IsCreated())
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T GetValue()
		{
			var allocator = GetAllocator();
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref _cachedPtr.Value();
		}

		[INLINE(256)]
		public ref T GetValue(Allocator allocator)
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetSafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref _cachedPtr.Value();
		}

		[INLINE(256)]
		public ProxyPtr<TProxy> ToProxy<TProxy>() where TProxy : unmanaged, IProxy
		{
			return new ProxyPtr<TProxy>(this);
		}

		[INLINE(256)]
		public Ptr<T> CopyTo(Allocator srsAllocator, Allocator dstAllocator)
		{
			return new Ptr<T>(memPtr.CopyTo(srsAllocator, dstAllocator));
		}

		[INLINE(256)]
		public void Dispose(Allocator allocator)
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

		[INLINE(256)]
		public bool Equals(Ptr<T> other)
		{
			return memPtr == other.memPtr;
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return memPtr.GetHashCode();
		}
	}

	public static class PtrExt
	{
		[INLINE(256)]
		public static Ptr<T> CreatePtr<T>(this Allocator allocator) where T : unmanaged
		{
			return Ptr<T>.Create(allocator);
		}
	}
}
