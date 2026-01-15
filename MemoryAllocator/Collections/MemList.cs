using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(MemList<>.ListProxy))]
	public struct MemList<T> : IMemListEnumerable<T> where T : unmanaged
	{
		private MemArray<T> _arr;
		private int _count;

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _count;
		}

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _arr.IsCreated;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _arr.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemList(int capacity = 0) : this(WorldManager.CurrentWorldState, capacity)
		{

		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemList(WorldState worldState, int capacity = 0)
		{
			this = default;
			EnsureCapacity(worldState, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemList(WorldState worldState, IEnumerable<T> enumerable, int capacity) : this(worldState, capacity)
		{
			AddRange(worldState, enumerable);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemList(WorldState worldState, MemListEnumerable<T> enumerable, int capacity) : this(worldState, capacity)
		{
			AddRange(worldState, enumerable);
		}

		public ref T Last(WorldState worldState)
		{
			return ref _arr[worldState, _count - 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ReplaceWith(WorldState worldState, in MemList<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
			{
				return;
			}

			Dispose(worldState);
			this = other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void CopyFrom(WorldState worldState, in MemList<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
				return;
			if (!_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
				return;
			if (_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
			{
				Dispose(worldState);
				return;
			}

			if (!_arr.innerArray.ptr.memPtr.IsValid())
				this = new MemList<T>(worldState, other.Capacity);

			MemArrayExt.Copy(worldState, in other._arr.innerArray, ref _arr.innerArray);
			_count = other._count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			return _arr.GetValuePtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> GetSpan(WorldState worldState)
		{
			var span = _arr.GetSpan(worldState);
			return span[.._count];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_arr.Dispose(worldState);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_count = 0;
		}

		public ref T this[WorldState worldState, int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _arr[worldState, index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState, int index)
		{
			return _arr.GetValuePtr(worldState, index);
		}

		/// <summary>
		/// Возвращает true, если состояние изменилось
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool EnsureCapacity(WorldState worldState, int capacity)
		{
			if (capacity <= Capacity)
			{
				if (_arr.IsCreated)
					return false;
				_arr = new MemArray<T>(worldState, capacity, ClearOptions.UninitializedMemory);
				return true;
			}
			capacity = capacity.NextPowerOfTwo();
			return _arr.Resize(worldState, capacity, ClearOptions.UninitializedMemory);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void EnsureCount(WorldState worldState, int count, in T defaultValue = default)
		{
			EnsureCapacity(worldState, count);
			if (_count < count)
			{
				_arr.Fill(worldState, defaultValue, _count, count - _count);
				_count = count;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetCount(WorldState worldState, int count)
		{
			EnsureCount(worldState, count);
			_count = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SetCountNoCheck(int count)
		{
			_count = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(WorldState worldState, in T value)
		{
			EnsureCapacity(worldState, _count + 1);

			E.ASSERT(IsCreated);
			_arr[worldState, _count] = value;
			_count++;
		}

		public void Insert(WorldState worldState, int index, T value)
		{
			EnsureCapacity(worldState, _count + 1);

			var source = GetValuePtr(worldState, index);

			if (_count > index)
			{
				var destination = source + 1;
				MemoryExt.MemMove<T>(source, destination, _count - index);
			}

			source.Value() = value;
			_count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly bool Contains<TU>(WorldState worldState, TU obj) where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);
			for (var i = 0; i < _count; ++i)
			{
				if (obj.Equals(_arr[worldState, i]))
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<TU>(WorldState worldState, TU obj) where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);
			for (var i = 0; i < _count; ++i)
			{
				if (obj.Equals(_arr[worldState, i]))
				{
					RemoveAt(worldState, i);
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveSwapBack<TU>(WorldState worldState, TU obj) where TU : unmanaged, IEquatable<T>
		{
			for (var i = 0; i < _count; ++i)
			{
				if (obj.Equals(_arr[worldState, i]))
				{
					RemoveAtSwapBack(worldState, i);
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveAt(WorldState worldState, int index)
		{
			if (index >= _count)
				return false;

			var endCount = index + 1;
			if (endCount != _count)
			{
				var ptr = _arr.innerArray.ptr.memPtr;
				worldState.MemMove<T>(ptr, endCount, ptr, index, _count - endCount);
			}

			--_count;
			_arr[worldState, _count] = default;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveAt(WorldState worldState, int index, int countToRemove)
		{
			var endCount = index + countToRemove;
			if (endCount > _count)
				return false;

			if (endCount != _count)
			{
				var ptr = _arr.innerArray.ptr.memPtr;
				worldState.MemMove<T>(ptr, endCount, ptr, index, (_count - endCount));
			}

			_count -= countToRemove;
			_arr.Fill(worldState, default, _count, countToRemove);

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveAtSwapBack(WorldState worldState, int index)
		{
			if (index >= _count)
				return false;

			--_count;
			var last = _arr[worldState, _count];
			_arr[worldState, index] = last;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool RemoveAtSwapBack(WorldState worldState, int index, int countToRemove)
		{
			if (index + countToRemove > _count)
				return false;

			var ptr = _arr.innerArray.ptr.memPtr;
			worldState.MemMove<T>(ptr, _count - countToRemove, ptr, index, countToRemove);
			_count -= countToRemove;

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Resize(WorldState worldState, int newLength)
		{
			if (!IsCreated)
			{
				this = new MemList<T>(worldState, newLength);
				return true;
			}

			if (newLength <= Capacity)
			{
				return false;
			}

			return EnsureCapacity(worldState, newLength);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange(WorldState worldState, T[] collection)
		{
			AddRange(worldState, collection.AsSpan());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange(WorldState worldState, Span<T> collection)
		{
			if (collection.Length == 0)
				return;

			EnsureCapacity(worldState, _count + collection.Length);
			E.ASSERT(IsCreated);

			var ptr = _arr.GetValuePtr(worldState, _count);
			collection.CopyTo(ptr.GetSpan(collection.Length));

			_count += collection.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange<TEnumerable>(WorldState worldState, TEnumerable collection) where TEnumerable: IEnumerable<T>
		{
			foreach (var value in collection)
			{
				Add(worldState, value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddRange(WorldState worldState, MemListEnumerable<T> collection)
		{
			foreach (var value in collection)
			{
				Add(worldState, value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void CopyTo(WorldState worldState, MemArray<T> arr, int srcOffset, int index, int count)
		{
			worldState.MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arr.innerArray.ptr.memPtr, index, count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly void CopyTo(WorldState worldState, in MemPtr arrPtr, int srcOffset, int index, int count)
		{
			worldState.MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arrPtr, index, count);
		}

		public int GetReservedSizeInBytes()
		{
			return _arr.GetReservedSizeInBytes();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemListEnumerator<T>(IsCreated ? GetValuePtr(worldState) : default,0 , Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}

		private class ListProxy
		{
			private MemList<T> _list;

			public ListProxy(MemList<T> list)
			{
				_list = list;
			}

			public int Capacity => _list.Capacity;

			public int Count => _list.Count;

			public T[] Items
			{
				get
				{
#if DEBUG
					var worldState = _list._arr.GetWorldState_DEBUG();
					var arr = new T[_list.Count];
					for (int i = 0; i < _list.Count; ++i)
					{
						arr[i] = _list[worldState, i];
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
