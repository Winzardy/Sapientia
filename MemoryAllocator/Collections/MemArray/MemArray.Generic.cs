using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;

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
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => innerArray.Length;
		}

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => innerArray.IsCreated;
		}

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => innerArray.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(int length, ClearOptions clearOptions = ClearOptions.ClearMemory) :
			this(WorldManager.CurrentWorldState, length, clearOptions)
		{

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(WorldState worldState, int length, T defaultValue)
		{
			innerArray = new MemArray(worldState, TSize<T>.size, length, ClearOptions.UninitializedMemory);
			innerArray.Fill(worldState, defaultValue, 0, innerArray.Length);
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(WorldState worldState, int length, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			innerArray = new MemArray(worldState, TSize<T>.size, length, clearOptions);
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(int elementSize, int length, ClearOptions clearOptions) :
			this(WorldManager.CurrentWorldState, elementSize, length, clearOptions)
		{

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(WorldState worldState, int elementSize, int length, ClearOptions clearOptions)
		{
			innerArray = new MemArray(worldState, elementSize, length, clearOptions);
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(in MemArray<T> arr) : this(WorldManager.CurrentWorldState, arr)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemArray(WorldState worldState, MemArray<T> arr)
		{
			innerArray = new MemArray(worldState, arr.As<MemArray<T>, MemArray>());
#if DEBUG
			_worldId = worldState.WorldId;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TU As<TU>(WorldState worldState, int index) where TU : unmanaged
		{
			return ref innerArray.GetValue<TU>(worldState, index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReplaceWith(WorldState worldState, MemArray<T> other)
		{
			innerArray.ReplaceWith(worldState, other.As<MemArray<T>, MemArray>());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(WorldState worldState, MemArray<T> other)
		{
			innerArray.CopyFrom(worldState, other.As<MemArray<T>, MemArray>());;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			innerArray.Dispose(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr(WorldState worldState)
		{
			return innerArray.GetPtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemPtr GetMemPtr(int index)
		{
			return innerArray.GetWPtr(index);
		}

		public ref T this[WorldState worldState, int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				E.ASSERT(IsCreated);
				E.RANGE(index, 0, this.Length);
				return ref innerArray.GetValue<T> (worldState, index);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			E.ASSERT(IsCreated);
			return innerArray.GetValuePtr<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> GetSpan(WorldState worldState)
		{
			return innerArray.GetSpan<T>(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState, int index)
		{
			return innerArray.GetValuePtr<T>(worldState, index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Resize(WorldState worldState, int newLength, ClearOptions options = ClearOptions.ClearMemory)
		{
			return innerArray.Resize<T>(worldState, newLength, options);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill(WorldState worldState, in T value)
		{
			innerArray.Fill(worldState, value, 0, Length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Fill(WorldState worldState, in T value, int fromIndex, int count)
		{
			innerArray.Fill(worldState, value, fromIndex, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState)
		{
			innerArray.Clear(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear(WorldState worldState, int index, int length)
		{
			innerArray.Clear(worldState, index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains<TU>(WorldState worldState, in TU obj) where TU : unmanaged, System.IEquatable<T>
		{
			return innerArray.Contains<T, TU>(worldState, obj);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetReservedSizeInBytes()
		{
			return innerArray.GetReservedSizeInBytes();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new ListEnumerator<T>(GetValuePtr(worldState), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
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
