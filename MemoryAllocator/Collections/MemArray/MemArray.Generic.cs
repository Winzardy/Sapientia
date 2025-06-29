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
			this(WorldManager.CurrentWorldState, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(WorldState worldState, int length, T defaultValue)
		{
			innerArray = new MemArray(worldState, TSize<T>.size, length, ClearOptions.UninitializedMemory);
			innerArray.Fill(worldState, defaultValue, 0, innerArray.Length);
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[INLINE(256)]
		public MemArray(WorldState worldState, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			innerArray = new MemArray(worldState, TSize<T>.size, length, clearOptions);
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[INLINE(256)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions) :
			this(WorldManager.CurrentWorldState, elementSize, length, clearOptions)
		{

		}

		[INLINE(256)]
		public MemArray(WorldState worldState, int elementSize, int length, ClearOptions clearOptions)
		{
			innerArray = new MemArray(worldState, elementSize, length, clearOptions);
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[INLINE(256)]
		public MemArray(in MemArray<T> arr) : this(WorldManager.CurrentWorldState, arr)
		{
		}

		[INLINE(256)]
		public MemArray(WorldState worldState, MemArray<T> arr)
		{
			innerArray = new MemArray(worldState, arr.As<MemArray<T>, MemArray>());
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[INLINE(256)]
		public MemArray(WorldState worldState, ReadOnlySpan<T> arr)
		{
			innerArray = new MemArray(worldState, TSize<T>.size, arr.Length, ClearOptions.UninitializedMemory);
			arr.CopyTo(new Span<T>(innerArray.GetValuePtr(worldState).ptr, arr.Length));
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

#if DEBUG
		internal WorldState GetWorldState_DEBUG()
		{
			return _worldId.GetWorldState();
		}
#endif

		[INLINE(256)]
		public ref TU As<TU>(WorldState worldState, int index) where TU : unmanaged
		{
			return ref innerArray.GetValue<TU>(worldState, index);
		}

		[INLINE(256)]
		public void ReplaceWith(WorldState worldState, MemArray<T> other)
		{
			innerArray.ReplaceWith(worldState, other.As<MemArray<T>, MemArray>());
		}

		[INLINE(256)]
		public void CopyFrom(WorldState worldState, MemArray<T> other)
		{
			innerArray.CopyFrom(worldState, other.As<MemArray<T>, MemArray>());;
		}

		[INLINE(256)]
		public void Dispose(WorldState worldState)
		{
			innerArray.Dispose(worldState);
		}

		[INLINE(256)]
		public SafePtr GetPtr(WorldState worldState)
		{
			return innerArray.GetPtr(worldState);
		}

		[INLINE(256)]
		public MemPtr GetMemPtr(int index)
		{
			return innerArray.GetWPtr(index);
		}

		public ref T this[WorldState worldState, int index]
		{
			[INLINE(256)]
			get
			{
				E.ASSERT(IsCreated);
				E.RANGE(index, 0, this.Length);
				return ref innerArray.GetValue<T> (worldState, index);
			}
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			E.ASSERT(IsCreated);
			return innerArray.GetValuePtr<T>(worldState);
		}

		[INLINE(256)]
		public Span<T> GetSpan(WorldState worldState)
		{
			return innerArray.GetSpan<T>(worldState);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(WorldState worldState, int index)
		{
			return innerArray.GetValuePtr<T>(worldState, index);
		}

		[INLINE(256)]
		public bool Resize(WorldState worldState, int newLength, ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize<T>(worldState, newLength, options);
		}

		[INLINE(256)]
		public void Fill(WorldState worldState, in T value)
		{
			innerArray.Fill(worldState, value, 0, Length);
		}

		[INLINE(256)]
		public void Fill(WorldState worldState, in T value, int fromIndex, int count)
		{
			innerArray.Fill(worldState, value, fromIndex, count);
		}

		[INLINE(256)]
		public void Clear(WorldState worldState)
		{
			innerArray.Clear(worldState);
		}

		[INLINE(256)]
		public void Clear(WorldState worldState, int index, int length)
		{
			innerArray.Clear(worldState, index, length);
		}

		[INLINE(256)]
		public bool Contains<TU>(WorldState worldState, in TU obj) where TU : unmanaged, System.IEquatable<T>
		{
			return innerArray.Contains<T, TU>(worldState, obj);
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return innerArray.GetReservedSizeInBytes();
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new ListEnumerator<T>(GetValuePtr(worldState), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator<T> GetPtrEnumerator(WorldState worldState)
		{
			return new ListPtrEnumerator<T>(GetValuePtr(worldState), 0, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(WorldState worldState)
		{
			return new (new (GetValuePtr(worldState), Count));
		}

		[INLINE(256)]
		public Enumerable<SafePtr<T>, ListPtrEnumerator<T>> GetPtrEnumerable(WorldState worldState)
		{
			return new (new (GetValuePtr(worldState), 0, Count));
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
					var worldState = _arr._worldId.GetWorldState();
					var arr = new T[_arr.Length];
					for (var i = 0; i < _arr.Length; ++i)
					{
						arr[i] = _arr[worldState, i];
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
