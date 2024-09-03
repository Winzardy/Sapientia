using System.Runtime.InteropServices;
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
		private void* _cachedPtr;
		public MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public Ptr(MemPtr memPtr) : this(0, null, memPtr) {}

		[INLINE(256)]
		public Ptr(ushort version, void* cachedPtr, MemPtr memPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public Ptr(Allocator* allocator, void* cachedPtr, MemPtr memPtr)
		{
			_version = allocator->version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public bool IsValid(Allocator* allocator)
		{
			return _version == allocator->version;
		}

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return memPtr.GetAllocatorPtr();
		}

		[INLINE(256)]
		public void* GetPtr()
		{
			var allocator = memPtr.GetAllocatorPtr();
			if (allocator->version != _version)
			{
				_cachedPtr = allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public void* GetPtr(Allocator* allocator)
		{
			if (allocator->version != _version)
			{
				_cachedPtr = allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public T* GetPtr<T>(Allocator* allocator) where T: unmanaged
		{
			if (allocator->version != _version)
			{
				_cachedPtr = allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return (T*)_cachedPtr;
		}

		[INLINE(256)]
		public ref T Get<T>() where T : unmanaged
		{
			var allocator = memPtr.GetAllocatorPtr();
			if (allocator->version != _version)
			{
				_cachedPtr = allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *(T*)_cachedPtr;
		}

		[INLINE(256)]
		public ref T Get<T>(Allocator* allocator) where T : unmanaged
		{
			if (allocator->version != _version)
			{
				_cachedPtr = allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *(T*)_cachedPtr;
		}

		[INLINE(256)]
		public ref T Get<T>(int index) where T : unmanaged
		{
			var allocator = memPtr.GetAllocatorPtr();
			if (allocator->version != _version)
			{
				_cachedPtr = allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *((T*)_cachedPtr + index);
		}

		[INLINE(256)]
		public ref T Get<T>(Allocator* allocator, int index) where T : unmanaged
		{
			if (allocator->version != _version)
			{
				_cachedPtr = allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *((T*)_cachedPtr + index);
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
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct Ptr<T> where T : unmanaged
	{
		public static readonly Ptr<T> Invalid = new (MemPtr.Invalid);

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ushort _version;
		private T* _cachedPtr;
		public MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public Ptr(MemPtr memPtr) : this(0, null, memPtr) {}

		[INLINE(256)]
		public Ptr(ushort version, T* cachedPtr, MemPtr memPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public Ptr(Allocator* allocator, T* cachedPtr, MemPtr memPtr)
		{
			_version = allocator->version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public static Ptr<T> Create(Allocator* allocator)
		{
			var memPtr = allocator->Alloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, cachedPtr, memPtr);
		}

		[INLINE(256)]
		public static Ptr<T> Create()
		{
			var allocator = AllocatorManager.CurrentAllocatorPtr;
			var memPtr = allocator->Alloc<T>(out var cachedPtr);
			return new Ptr<T>(allocator, cachedPtr, memPtr);
		}

		[INLINE(256)]
		public Ptr<T1> ToCachedPtr<T1>() where T1 : unmanaged
		{
			return new Ptr<T1>(_version, (T1*)_cachedPtr, memPtr);
		}

		[INLINE(256)]
		public bool IsValid(Allocator* allocator)
		{
			return _version == allocator->version;
		}

		[INLINE(256)]
		public Allocator* GetAllocatorPtr()
		{
			return memPtr.allocatorId.GetAllocatorPtr();
		}

		[INLINE(256)]
		public T* GetPtr()
		{
			var allocator = GetAllocatorPtr();
			if (allocator->version != _version)
			{
				_cachedPtr = (T*)allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public T* GetPtr(Allocator* allocator)
		{
			if (allocator->version != _version)
			{
				_cachedPtr = (T*)allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T GetValue()
		{
			var allocator = GetAllocatorPtr();
			if (allocator->version != _version)
			{
				_cachedPtr = (T*)allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *_cachedPtr;
		}

		[INLINE(256)]
		public ref T GetValue(Allocator* allocator)
		{
			if (allocator->version != _version)
			{
				_cachedPtr = (T*)allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *_cachedPtr;
		}

		[INLINE(256)]
		public ref T GetValue(Allocator* allocator, int index)
		{
			if (allocator->version != _version)
			{
				_cachedPtr = (T*)allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *(_cachedPtr + index);
		}

		[INLINE(256)]
		public ref T GetValue(int index)
		{
			var allocator = GetAllocatorPtr();
			if (allocator->version != _version)
			{
				_cachedPtr = (T*)allocator->GetUnsafePtr(in memPtr);
				_version = allocator->version;
			}

			return ref *(_cachedPtr + index);
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
		public static implicit operator ValueRef(Ptr<T> value)
		{
			return ValueRef.Create(value);
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
	}
}
