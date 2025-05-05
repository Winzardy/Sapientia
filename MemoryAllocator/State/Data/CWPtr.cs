using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.TypeIndexer;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Cached World Ptr
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CWPtr
	{
		public static readonly CWPtr Invalid = new (WPtr.Invalid);

		private ushort _version;
		private SafePtr _cachedPtr;
		public WPtr wPtr;

		[INLINE(256)]
		public readonly bool IsCreated() => wPtr.IsCreated();
		[INLINE(256)]
		public bool IsValid() => wPtr.IsValid();

		[INLINE(256)]
		public CWPtr(WPtr wPtr) : this(0, default, wPtr) {}

		[INLINE(256)]
		public CWPtr(ushort version, SafePtr cachedPtr, WPtr wPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.wPtr = wPtr;
		}

		[INLINE(256)]
		public CWPtr(World world, SafePtr cachedPtr, WPtr wPtr)
		{
			_version = world.version;
			_cachedPtr = cachedPtr;
			this.wPtr = wPtr;
		}

		[INLINE(256)]
		public bool IsValid(World world)
		{
			return _version == world.version;
		}

		[INLINE(256)]
		public SafePtr GetPtr(World world)
		{
			if (world.version != _version)
			{
				_cachedPtr = world.GetSafePtr(wPtr);
				_version = world.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr<T>(World world) where T: unmanaged
		{
			if (world.version != _version)
			{
				_cachedPtr = world.GetSafePtr(wPtr);
				_version = world.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T Get<T>(World world) where T : unmanaged
		{
			if (world.version != _version)
			{
				_cachedPtr = world.GetSafePtr(wPtr);
				_version = world.version;
			}

			return ref _cachedPtr.Value<T>();
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			wPtr.Dispose(world);
			this = Invalid;
		}

		public CWPtr CopyTo(World srsWorld, World dstWorld)
		{
			return new CWPtr(wPtr.CopyTo(srsWorld, dstWorld));
		}

		[INLINE(256)]
		public static implicit operator CWPtr(WPtr value)
		{
			return new CWPtr(value);
		}

		public static bool operator ==(CWPtr a, CWPtr b)
		{
			return a.wPtr == b.wPtr;
		}

		public static bool operator !=(CWPtr a, CWPtr b)
		{
			return a.wPtr != b.wPtr;
		}

		public override int GetHashCode()
		{
			return wPtr.GetHashCode();
		}
	}

	/// <summary>
	/// Cached World Ptr
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct CWPtr<T> : IEquatable<CWPtr<T>> where T : unmanaged
	{
		public static readonly CWPtr<T> Invalid = new (WPtr.Invalid);

#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		private ushort _version;
		private SafePtr<T> _cachedPtr;
		public WPtr wPtr;

		[INLINE(256)]
		public readonly bool IsCreated() => wPtr.IsCreated();
		[INLINE(256)]
		public bool IsValid() => wPtr.IsValid();

		[INLINE(256)]
		public CWPtr(WPtr wPtr) : this(0, wPtr, default) {}

		[INLINE(256)]
		public CWPtr(ushort version, WPtr wPtr, SafePtr<T> cachedPtr)
		{
			_version = version;
			_cachedPtr = cachedPtr;
			this.wPtr = wPtr;
		}

		[INLINE(256)]
		public CWPtr(World world, WPtr wPtr, SafePtr<T> cachedPtr)
		{
			_version = world.version;
			_cachedPtr = cachedPtr;
			this.wPtr = wPtr;
		}

		[INLINE(256)]
		public CWPtr(World world, WPtr wPtr, SafePtr<T> cachedPtr, in T value)
		{
			_version = world.version;
			_cachedPtr = cachedPtr;
			this.wPtr = wPtr;

			cachedPtr[0] = value;
		}

		[INLINE(256)]
		public static CWPtr<T> Create(World world)
		{
			var memPtr = world.MemAlloc<T>(out var cachedPtr);
			return new CWPtr<T>(world, memPtr, cachedPtr);
		}

		[INLINE(256)]
		public static CWPtr<T> Create(World world, in T value)
		{
			var memPtr = world.MemAlloc<T>(out var cachedPtr);
			return new CWPtr<T>(world, memPtr, cachedPtr, value);
		}

		[INLINE(256)]
		public static SafePtr<T> Create(World world, out CWPtr<T> ptr)
		{
			var memPtr = world.MemAlloc<T>(out var cachedPtr);
			ptr = new CWPtr<T>(world, memPtr, cachedPtr);
			return cachedPtr;
		}

		[INLINE(256)]
		public static CWPtr<T> Create()
		{
			var allocator = WorldManager.CurrentWorld;
			var memPtr = allocator.MemAlloc<T>(out var cachedPtr);
			return new CWPtr<T>(allocator, memPtr, cachedPtr);
		}

		[INLINE(256)]
		public CWPtr<T1> ToCachedPtr<T1>() where T1 : unmanaged
		{
			return new CWPtr<T1>(_version, wPtr, _cachedPtr.Cast<T1>());
		}

		[INLINE(256)]
		public SafePtr<T> GetPtr(World world)
		{
			if (world.version != _version && wPtr.IsCreated())
			{
				_cachedPtr = world.GetSafePtr(wPtr);
				_version = world.version;
			}

			return _cachedPtr;
		}

		[INLINE(256)]
		public ref T GetValue(World world)
		{
			if (world.version != _version)
			{
				_cachedPtr = world.GetSafePtr(wPtr);
				_version = world.version;
			}

			return ref _cachedPtr.Value();
		}

		[INLINE(256)]
		public ProxyPtr<TProxy> ToProxy<TProxy>() where TProxy : unmanaged, IProxy
		{
			return new ProxyPtr<TProxy>(this);
		}

		[INLINE(256)]
		public CWPtr<T> CopyTo(World srsWorld, World dstWorld)
		{
			return new CWPtr<T>(wPtr.CopyTo(srsWorld, dstWorld));
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			wPtr.Dispose(world);
			this = Invalid;
		}

		[INLINE(256)]
		public static implicit operator CWPtr(CWPtr<T> value)
		{
			return value.As<CWPtr<T>, CWPtr>();
		}

		[INLINE(256)]
		public static implicit operator CWPtr<T>(CWPtr value)
		{
			return value.As<CWPtr, CWPtr<T>>();
		}

		[INLINE(256)]
		public static implicit operator CWPtr<T>(WPtr value)
		{
			return new CWPtr<T>(value);
		}

		[INLINE(256)]
		public static implicit operator IndexedPtr(CWPtr<T> value)
		{
			return IndexedPtr.Create(value);
		}

		[INLINE(256)]
		public static implicit operator CWPtr<T>(IndexedPtr value)
		{
			return value.GetCachedPtr();
		}

		[INLINE(256)]
		public static bool operator ==(CWPtr<T> a, CWPtr<T> b)
		{
			return a.wPtr == b.wPtr;
		}

		[INLINE(256)]
		public static bool operator !=(CWPtr<T> a, CWPtr<T> b)
		{
			return a.wPtr != b.wPtr;
		}

		[INLINE(256)]
		public bool Equals(CWPtr<T> other)
		{
			return wPtr == other.wPtr;
		}

		[INLINE(256)]
		public override int GetHashCode()
		{
			return wPtr.GetHashCode();
		}
	}

	public static class PtrExt
	{
		[INLINE(256)]
		public static CWPtr<T> CreatePtr<T>(this World world) where T : unmanaged
		{
			return CWPtr<T>.Create(world);
		}
	}
}
