using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Sequential)]
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemArray<>.MemArrayProxy))]
	public unsafe struct MemArray<T> : IListEnumerable<T> where T : unmanaged
	{
		public static readonly MemArray<T> Empty = new () { innerArray = MemArray.Empty };

		public MemArray innerArray;
#if DEBUG
		private WorldId _worldId;
#endif

		public readonly int Length
		{
			[INLINE(256)] get => innerArray.Length;
		}

		public readonly bool IsCreated
		{
			[INLINE(256)] get => innerArray.IsCreated;
		}

		public readonly int Count
		{
			[INLINE(256)] get => innerArray.Length;
		}

		[INLINE(256)]
		public MemArray(int length, ClearOptions clearOptions = ClearOptions.ClearMemory) :
			this(WorldManager.CurrentWorld, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(World world, int length, T defaultValue)
		{
			innerArray = new MemArray(world, TSize<T>.size, length, ClearOptions.UninitializedMemory);
			innerArray.Fill(world, defaultValue, 0, innerArray.Length);
#if DEBUG
			_worldId = world.worldId;
#endif
		}

		[INLINE(256)]
		public MemArray(World world, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			innerArray = new MemArray(world, TSize<T>.size, length, clearOptions);
#if DEBUG
			_worldId = world.worldId;
#endif
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions) :
			this(WorldManager.CurrentWorld, elementSize, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(World world, int elementSize, int length, ClearOptions clearOptions)
		{
			innerArray = new MemArray(world, elementSize, length, clearOptions);
#if DEBUG
			_worldId = world.worldId;
#endif
		}

		[INLINE(256)]
		public MemArray(in MemArray<T> arr) : this(WorldManager.CurrentWorld, arr)
		{
		}

		[INLINE(256)]
		public MemArray(World world, MemArray<T> arr)
		{
			innerArray = new MemArray(world, arr.As<MemArray<T>, MemArray>());
#if DEBUG
			_worldId = world.worldId;
#endif
		}

		[INLINE(256)]
		public MemArray(World world, ReadOnlySpan<T> arr)
		{
			innerArray = new MemArray(world, TSize<T>.size, arr.Length, ClearOptions.UninitializedMemory);
			arr.CopyTo(new Span<T>(innerArray.GetValuePtr(world).ptr, arr.Length));
#if DEBUG
			_worldId = world.worldId;
#endif
		}

#if DEBUG
		internal World GetWorld_DEBUG()
		{
			return _worldId.GetWorld();
		}
#endif

		[INLINE(256)]
		public ref TU As<TU>(World world, int index) where TU : unmanaged
		{
			return ref innerArray.GetValue<TU>(world, index);
		}

		[INLINE(256)]
		public void ReplaceWith(World world, MemArray<T> other)
		{
			innerArray.ReplaceWith(world, other.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public void CopyFrom(World world, MemArray<T> other)
		{
			innerArray.CopyFrom(world, other.As<MemArray<T>, MemArray>());;
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			innerArray.Dispose(world);
		}

		[INLINE(256)]
		public SafePtr GetPtr(World world)
		{
			return innerArray.GetPtr(world);
		}

		[INLINE(256)]
		public MemPtr GetMemPtr(int index)
		{
			return innerArray.GetWPtr(index);
		}

		public ref T this[World world, int index]
		{
			[INLINE(256)]
			get
			{
				E.ASSERT(IsCreated);
				E.RANGE(index, 0, this.Length);
				return ref innerArray.GetValue<T> (world, index);
			}
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(World world)
		{
			E.ASSERT(IsCreated);
			return innerArray.GetValuePtr<T>(world);
		}

		[INLINE(256)]
		public Span<T> GetSpan(World world)
		{
			return innerArray.GetSpan<T>(world);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(World world, int index)
		{
			return innerArray.GetValuePtr<T>(world, index);
		}

		[INLINE(256)]
		public bool Resize(World world, int newLength, ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize<T>(world, newLength, options);
		}

		[INLINE(256)]
		public void Fill(World world, in T value, int fromIndex, int count)
		{
			innerArray.Fill(world, value, fromIndex, count);
		}

		[INLINE(256)]
		public void Clear(World world)
		{
			innerArray.Clear(world);
		}

		[INLINE(256)]
		public void Clear(World world, int index, int length)
		{
			innerArray.Clear(world, index, length);
		}

		[INLINE(256)]
		public bool Contains<TU>(World world, in TU obj) where TU : unmanaged, System.IEquatable<T>
		{
			return innerArray.Contains<T, TU>(world, obj);
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return innerArray.GetReservedSizeInBytes();
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator(World world)
		{
			return new ListEnumerator<T>(GetValuePtr(world), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator(World world)
		{
			return new ListPtrEnumerator<T>(GetValuePtr(world), 0, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(World world)
		{
			return new (new (GetValuePtr(world), Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable(World world)
		{
			return new (new (GetValuePtr(world), 0, Count));
		}

		private class MemArrayProxy
		{
			private MemArray<T> _arr;

			public MemArrayProxy(MemArray<T> arr)
			{
				_arr = arr;
			}

			public T[] Items
			{
				get
				{
#if DEBUG
					var world = _arr._worldId.GetWorld();
					var arr = new T[_arr.Length];
					for (var i = 0; i < _arr.Length; ++i)
					{
						arr[i] = _arr[world, i];
					}
					return arr;
#else
					return Array.Empty<T>();
#endif
				}
			}
		}
	}
}
