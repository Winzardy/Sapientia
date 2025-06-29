using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(List<>.ListProxy))]
	public struct List<T> : IListEnumerable<T> where T : unmanaged
	{
		private MemArray<T> _arr;
		private int _count;

		public readonly int Count
		{
			[INLINE(256)] get => _count;
		}

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _arr.IsCreated;
		}

		public int Capacity
		{
			[INLINE(256)]
			get => _arr.Length;
		}

		[INLINE(256)]
		public List(int capacity = 0) : this(WorldManager.CurrentWorldState, capacity)
		{

		}

		[INLINE(256)]
		public List(WorldState worldState, int capacity = 0)
		{
			this = default;
			EnsureCapacity(worldState, capacity);

			if (!_arr.IsCreated)
				_arr = new MemArray<T>(worldState, capacity);
		}

		[INLINE(256)]
		public List(WorldState worldState, IEnumerable<T> enumerable, int capacity) : this(worldState, capacity)
		{
			AddRange(worldState, enumerable);
		}

		[INLINE(256)]
		public void ReplaceWith(WorldState worldState, in List<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
			{
				return;
			}

			Dispose(worldState);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(WorldState worldState, in List<T> other)
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
				this = new List<T>(worldState, other.Capacity);

			MemArrayExt.Copy(worldState, in other._arr.innerArray, ref _arr.innerArray);
			_count = other._count;
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			return _arr.GetValuePtr(worldState);
		}

		[INLINE(256)]
		public Span<T> GetSpan(WorldState worldState)
		{
			var span = _arr.GetSpan(worldState);
			return span[.._count];
		}

		[INLINE(256)]
		public void Dispose(WorldState worldState)
		{
			_arr.Dispose(worldState);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			_count = 0;
		}

		public ref T this[WorldState worldState, int index]
		{
			[INLINE(256)]
			get => ref _arr[worldState, index];
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(WorldState worldState, int index)
		{
			return _arr.GetValuePtr(worldState, index);
		}

		[INLINE(256)]
		public bool EnsureCapacity(WorldState worldState, int capacity)
		{
			if (capacity <= Capacity)
				return false;
			capacity = capacity.NextPowerOfTwo();
			return _arr.Resize(worldState, capacity, ClearOptions.UninitializedMemory);
		}

		[INLINE(256)]
		public void EnsureCount(WorldState worldState, int count, in T defaultValue = default)
		{
			EnsureCapacity(worldState, count);
			if (_count < count)
			{
				_arr.Fill(worldState, defaultValue, _count, count - _count);
				_count = count;
			}
		}

		[INLINE(256)]
		public void SetCount(WorldState worldState, int count)
		{
			EnsureCount(worldState, count);
			_count = count;
		}

		[INLINE(256)]
		public void SetCountNoCheck(int count)
		{
			_count = count;
		}

		[INLINE(256)]
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

		[INLINE(256)]
		public readonly bool Contains<TU>(WorldState worldState, TU obj) where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[worldState, i]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool Remove<TU>(WorldState worldState, TU obj) where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[worldState, i]))
				{
					RemoveAt(worldState, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveFast<TU>(WorldState worldState, TU obj) where TU : unmanaged, IEquatable<T>
		{
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[worldState, i]))
				{
					RemoveAtSwapBack(worldState, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveAt(WorldState worldState, int index)
		{
			if (index >= _count)
				return false;

			if (index == _count - 1)
			{
				--_count;
				_arr[worldState, _count] = default;
				return true;
			}

			var ptr = _arr.innerArray.ptr.memPtr;
			worldState.MemMove<T>(ptr, index + 1, ptr, index, (_count - index - 1));

			--_count;
			_arr[worldState, _count] = default;

			return true;
		}

		[INLINE(256)]
		public bool RemoveAtSwapBack(WorldState worldState, int index)
		{
			if (index >= _count)
				return false;

			--_count;
			var last = _arr[worldState, _count];
			_arr[worldState, index] = last;

			return true;
		}

		[INLINE(256)]
		public bool Resize(WorldState worldState, int newLength)
		{
			if (!IsCreated)
			{
				this = new List<T>(worldState, newLength);
				return true;
			}

			if (newLength <= Capacity)
			{
				return false;
			}

			return EnsureCapacity(worldState, newLength);
		}

		[INLINE(256)]
		public void AddRange<TEnumerable>(WorldState worldState, TEnumerable collection) where TEnumerable: IEnumerable<T>
		{
			foreach (var value in collection)
			{
				Add(worldState, value);
			}
		}

		[INLINE(256)]
		public readonly void CopyTo(WorldState worldState, MemArray<T> arr, int srcOffset, int index, int count)
		{
			worldState.MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arr.innerArray.ptr.memPtr, index, count);
		}

		[INLINE(256)]
		public readonly void CopyTo(WorldState worldState, in MemPtr arrPtr, int srcOffset, int index, int count)
		{
			worldState.MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arrPtr, index, count);
		}

		public int GetReservedSizeInBytes()
		{
			return _arr.GetReservedSizeInBytes();
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

		private class ListProxy
		{
			private List<T> _list;

			public ListProxy(List<T> list)
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
