using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Кеширует внутри себя сырой указатель на данные (Помимо указателя на область памяти в аллокаторе)
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CachedPtr : IEquatable<CachedPtr>
	{
		public static readonly CachedPtr Invalid = new (MemPtr.Invalid);

		private ushort _version;
		private SafePtr _cachedPtr;
		public MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public CachedPtr(MemPtr memPtr) : this(0, default, memPtr) {}

		[INLINE(256)]
		public CachedPtr(ushort version, SafePtr cachedPtr, MemPtr memPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public CachedPtr(World world, SafePtr cachedPtr, MemPtr memPtr)
		{
			_version = world.Version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public bool IsValid(World world)
		{
			return _version == world.Version;
		}

		[INLINE(256)]
		public SafePtr GetPtr(World world)
		{
			if (world.Version != _version)
			{
				_cachedPtr = world.GetSafePtr(memPtr);
				_version = world.Version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>(World world) where T: unmanaged
		{
			if (world.Version != _version)
			{
				_cachedPtr = world.GetSafePtr(memPtr);
				_version = world.Version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T Get<T>(World world) where T : unmanaged
		{
			if (world.Version != _version)
			{
				_cachedPtr = world.GetSafePtr(memPtr);
				_version = world.Version;
			}

			return ref _cachedPtr.Value<T>();
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			memPtr.Dispose(world);
			this = Invalid;
		}

		[INLINE(256)]
		public CachedPtr CopyTo(World srsWorld, World dstWorld)
		{
			return new CachedPtr(memPtr.CopyTo(srsWorld, dstWorld));
		}

		[INLINE(256)]
		public static implicit operator CachedPtr(MemPtr value)
		{
			return new CachedPtr(value);
		}

		[INLINE(256)]
		public static bool operator ==(CachedPtr a, CachedPtr b)
		{
			return a.memPtr == b.memPtr;
		}

		[INLINE(256)]
		public static bool operator !=(CachedPtr a, CachedPtr b)
		{
			return a.memPtr != b.memPtr;
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return memPtr.GetHashCode();
		}

		[INLINE(256)]
		public bool Equals(CachedPtr other)
		{
			return memPtr.Equals(other.memPtr);
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is CachedPtr other && Equals(other);
		}
	}

	/// <summary>
	/// Кеширует внутри себя сырой указатель на данные (Помимо указателя на область памяти в аллокаторе)
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CachedPtr<T> : IEquatable<CachedPtr<T>> where T : unmanaged
	{
		public static readonly CachedPtr<T> Invalid = new (MemPtr.Invalid);

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ushort _version;
		private SafePtr<T> _cachedPtr;
		public MemPtr memPtr;

		[INLINE(256)]
		public readonly bool IsValid() => memPtr.IsValid();

		[INLINE(256)]
		public CachedPtr(MemPtr memPtr) : this(0, memPtr, default) {}

		[INLINE(256)]
		public CachedPtr(ushort version, MemPtr memPtr, SafePtr<T> cachedPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public CachedPtr(World world, MemPtr memPtr, SafePtr<T> cachedPtr)
		{
			_version = world.Version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;
		}

		[INLINE(256)]
		public CachedPtr(World world, MemPtr memPtr, SafePtr<T> cachedPtr, in T value)
		{
			_version = world.Version;
			_cachedPtr = cachedPtr;
			this.memPtr = memPtr;

			cachedPtr[0] = value;
		}

		[INLINE(256)]
		public static CachedPtr<T> Create(World world)
		{
			var memPtr = world.MemAlloc<T>(out var cachedPtr);
			return new CachedPtr<T>(world, memPtr, cachedPtr);
		}

		[INLINE(256)]
		public static CachedPtr<T> Create(World world, in T value)
		{
			var memPtr = world.MemAlloc<T>(out var cachedPtr);
			return new CachedPtr<T>(world, memPtr, cachedPtr, value);
		}

		[INLINE(256)]
		public static SafePtr<T> Create(World world, out CachedPtr<T> ptr)
		{
			var memPtr = world.MemAlloc<T>(out var cachedPtr);
			ptr = new CachedPtr<T>(world, memPtr, cachedPtr);
			return cachedPtr;
		}

		[INLINE(256)]
		public static CachedPtr<T> Create()
		{
			var world = WorldManager.CurrentWorld;
			var memPtr = world.MemAlloc<T>(out var cachedPtr);
			return new CachedPtr<T>(world, memPtr, cachedPtr);
		}

		[INLINE(256)]
		public CachedPtr<T1> ToCachedPtr<T1>() where T1 : unmanaged
		{
			return new CachedPtr<T1>(_version, memPtr, _cachedPtr.Cast<T1>());
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr(World world)
		{
			if (world.Version != _version && memPtr.IsValid())
			{
				_cachedPtr = world.GetSafePtr(memPtr);
				_version = world.Version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T GetValue(World world)
		{
			if (world.Version != _version)
			{
				_cachedPtr = world.GetSafePtr(memPtr);
				_version = world.Version;
			}

			return ref _cachedPtr.Value();
		}

		[INLINE(256)]
		public ProxyPtr<TProxy> ToProxy<TProxy>() where TProxy : unmanaged, IProxy
		{
			return new ProxyPtr<TProxy>(this);
		}

		[INLINE(256)]
		public CachedPtr<T> CopyTo(World srsWorld, World dstWorld)
		{
			return new CachedPtr<T>(memPtr.CopyTo(srsWorld, dstWorld));
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			memPtr.Dispose(world);
			this = Invalid;
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

		[INLINE(256)]
		public static implicit operator CachedPtr<T>(MemPtr value)
		{
			return new CachedPtr<T>(value);
		}

		[INLINE(256)]
		public static implicit operator IndexedPtr(CachedPtr<T> value)
		{
			return IndexedPtr.Create(value);
		}

		[INLINE(256)]
		public static implicit operator CachedPtr<T>(IndexedPtr value)
		{
			return value.GetCachedPtr();
		}

		[INLINE(256)]
		public static bool operator ==(CachedPtr<T> a, CachedPtr<T> b)
		{
			return a.memPtr == b.memPtr;
		}

		[INLINE(256)]
		public static bool operator !=(CachedPtr<T> a, CachedPtr<T> b)
		{
			return a.memPtr != b.memPtr;
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return memPtr.GetHashCode();
		}

		[INLINE(256)]
		public bool Equals(CachedPtr<T> other)
		{
			return memPtr == other.memPtr;
		}

		[INLINE(256)]
		public override bool Equals(object obj)
		{
			return obj is CachedPtr<T> other && Equals(other);
		}
	}

	public static class PtrExt
	{
		[INLINE(256)]
		public static CachedPtr<T> CreatePtr<T>(this World world) where T : unmanaged
		{
			return CachedPtr<T>.Create(world);
		}
	}
}
