using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator.Data
{
	public unsafe struct CachedPtr
	{
		public static readonly CachedPtr Invalid = new (MemPtr.Invalid);

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ushort _version;
		private void* _cachedPtr;
		public readonly MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public CachedPtr(MemPtr memPtr) : this(0, null, memPtr) {}

		[INLINE(256)]
		public CachedPtr(ushort version, void* cachedPtr, MemPtr memPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public CachedPtr(in Allocator allocator, void* cachedPtr, MemPtr memPtr)
		{
			_version = allocator.version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public bool IsValid(in Allocator allocator)
		{
			return _version == allocator.version;
		}

		[INLINE(256)]
		public void* ReadPtr(in Allocator allocator)
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T Read<T>(in Allocator allocator) where T : unmanaged
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref *(T*)_cachedPtr;
		}

		[INLINE(256)]
		public ref T Read<T>(in Allocator allocator, int index) where T : unmanaged
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref *((T*)_cachedPtr + index);
		}

		[INLINE(256)]
		public ref T Read<T>(in Allocator allocator, uint index) where T : unmanaged
		{
			if (allocator.version != _version)
			{
				_cachedPtr = allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref *((T*)_cachedPtr + index);
		}
	}

	public unsafe struct CachedPtr<T> where T : unmanaged
	{
		public static readonly CachedPtr<T> Invalid = new (MemPtr.Invalid);

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ushort _version;
		private T* _cachedPtr;
		public readonly MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public CachedPtr(MemPtr memPtr) : this(0, null, memPtr) {}

		[INLINE(256)]
		public CachedPtr(ushort version, T* cachedPtr, MemPtr memPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public CachedPtr(in Allocator allocator, T* cachedPtr, MemPtr memPtr)
		{
			_version = allocator.version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public bool IsValid(in Allocator allocator)
		{
			return _version == allocator.version;
		}

		[INLINE(256)]
		public void* ReadPtr(in Allocator allocator)
		{
			if (allocator.version != _version)
			{
				_cachedPtr = (T*)allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T Read(in Allocator allocator)
		{
			if (allocator.version != _version)
			{
				_cachedPtr = (T*)allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref *_cachedPtr;
		}

		[INLINE(256)]
		public ref T Read(in Allocator allocator, int index)
		{
			if (allocator.version != _version)
			{
				_cachedPtr = (T*)allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref *(_cachedPtr + index);
		}

		[INLINE(256)]
		public ref T Read(in Allocator allocator, uint index)
		{
			if (allocator.version != _version)
			{
				_cachedPtr = (T*)allocator.GetUnsafePtr(in memPtr);
				_version = allocator.version;
			}

			return ref *(_cachedPtr + index);
		}

		[INLINE(256)]
		public static implicit operator CachedPtr(CachedPtr<T> value)
		{
			return value.As<CachedPtr<T>, CachedPtr>();
		}

		[INLINE(256)]
		public static implicit operator CachedPtr<T>(CachedPtr value)
		{
			return value.As<CachedPtr, CachedPtr<T>>();
		}
	}
}
