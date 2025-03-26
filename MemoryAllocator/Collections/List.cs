using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(ListProxy<>))]
	public unsafe struct List<T> : IListEnumerable<T> where T : unmanaged
	{
		private MemArray<T> _arr;
		private int _count;

		public readonly int Count
		{
			[INLINE(256)] get => _count;
		}

		public int ElementSize
		{
			[INLINE(256)] get => _arr.ElementSize;
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
		public Allocator* GetAllocatorPtr()
		{
			return _arr.GetAllocatorPtr();
		}

		[INLINE(256)]
		public List(int capacity = 0) : this(AllocatorManager.CurrentAllocatorPtr, capacity)
		{

		}

		[INLINE(256)]
		public List(Allocator* allocator, int capacity = 0)
		{
			this = default;
			EnsureCapacity(allocator, capacity);

			if (!_arr.IsCreated)
				_arr = new MemArray<T>(allocator, capacity);
		}

		[INLINE(256)]
		public List(Allocator* allocator, IEnumerable<T> enumerable, int capacity) : this(allocator, capacity)
		{
			AddRange(allocator, enumerable);
		}

		[INLINE(256)]
		public void ReplaceWith(Allocator* allocator, in List<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
			{
				return;
			}

			Dispose(allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(Allocator* allocator, in List<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
				return;
			if (!_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
				return;
			if (_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
			{
				Dispose(allocator);
				return;
			}

			if (!_arr.innerArray.ptr.memPtr.IsValid())
				this = new List<T>(allocator, other.Capacity);

			MemArrayExt.Copy(allocator, in other._arr.innerArray, ref _arr.innerArray);
			_count = other._count;
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			return _arr.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public T* GetValuePtr()
		{
			return _arr.GetValuePtr();
		}

		[INLINE(256)]
		public T* GetValuePtr(Allocator* allocator)
		{
			return _arr.GetValuePtr(allocator);
		}

		[INLINE(256)]
		public Span<T> GetSpan(Allocator* allocator)
		{
			var span = _arr.GetSpan(allocator);
			return span[.._count];
		}

		[INLINE(256)]
		public void Dispose(Allocator* allocator)
		{
			_arr.Dispose(allocator);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			_count = 0;
		}

		public ref T this[int index]
		{
			[INLINE(256)]
			get => ref _arr[GetAllocatorPtr(), index];
		}

		public ref T this[Allocator* allocator, int index]
		{
			[INLINE(256)]
			get => ref _arr[allocator, index];
		}

		[INLINE(256)]
		public T* GetValuePtr(Allocator* allocator, int index)
		{
			return _arr.GetValuePtr(allocator, index);
		}

		[INLINE(256)]
		public bool EnsureCapacity(int capacity)
		{
			return EnsureCapacity(GetAllocatorPtr(), capacity);
		}

		[INLINE(256)]
		public bool EnsureCapacity(Allocator* allocator, int capacity)
		{
			if (capacity <= Capacity)
				return false;
			capacity = capacity.NextPowerOfTwo();
			return _arr.Resize(allocator, capacity, ClearOptions.UninitializedMemory);
		}

		[INLINE(256)]
		public void EnsureCount(int count, in T defaultValue = default)
		{
			EnsureCount(GetAllocatorPtr(), count, defaultValue);
		}

		[INLINE(256)]
		public void EnsureCount(Allocator* allocator, int count, in T defaultValue = default)
		{
			EnsureCapacity(allocator, count);
			if (_count < count)
			{
				_arr.Fill(allocator, defaultValue, _count, count - _count);
				_count = count;
			}
		}

		[INLINE(256)]
		public void SetCount(Allocator* allocator, int count)
		{
			EnsureCount(allocator, count);
			_count = count;
		}

		[INLINE(256)]
		public void SetCountNoCheck(int count)
		{
			_count = count;
		}

		[INLINE(256)]
		public void Add(T obj)
		{
			Debug.Assert(IsCreated);

			var allocator = GetAllocatorPtr();
			++_count;
			EnsureCapacity(allocator, _count);

			_arr[allocator, _count - 1] = obj;
		}

		[INLINE(256)]
		public void Add(Allocator* allocator, in T obj)
		{
			Debug.Assert(IsCreated);
			EnsureCapacity(allocator, _count + 1);

			_arr[allocator, _count] = obj;
			_count++;
		}

		public void Insert(Allocator* allocator, int index, T value)
		{
			EnsureCapacity(allocator, _count + 1);

			var source = GetValuePtr(allocator, index);
			var destination = source + 1;
			MemoryExt.MemMove<T>(source, destination, _count - index);

			*source = value;
			_count++;
		}

		[INLINE(256)]
		public readonly bool Contains<TU>(Allocator* allocator, TU obj) where TU : unmanaged, IEquatable<T>
		{
			Debug.Assert(IsCreated);
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[allocator, i]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool Remove<TU>(Allocator* allocator, TU obj) where TU : unmanaged, IEquatable<T>
		{
			Debug.Assert(IsCreated);
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[allocator, i]))
				{
					RemoveAt(allocator, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveFast<TU>(Allocator* allocator, TU obj) where TU : unmanaged, IEquatable<T>
		{
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[allocator, i]))
				{
					RemoveAtSwapBack(allocator, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveAt(Allocator* allocator, int index)
		{
			if (index >= _count)
				return false;

			if (index == _count - 1)
			{
				--_count;
				_arr[allocator, _count] = default;
				return true;
			}

			var ptr = _arr.innerArray.ptr.memPtr;
			allocator->MemMove<T>(ptr, index + 1, ptr, index, (_count - index - 1));

			--_count;
			_arr[allocator, _count] = default;

			return true;
		}

		[INLINE(256)]
		public bool RemoveAtSwapBack(int index)
		{
			return RemoveAtSwapBack(GetAllocatorPtr(), index);
		}

		[INLINE(256)]
		public bool RemoveAtSwapBack(Allocator* allocator, int index)
		{
			if (index >= _count)
				return false;

			--_count;
			var last = _arr[allocator, _count];
			_arr[allocator, index] = last;

			return true;
		}

		[INLINE(256)]
		public bool Resize(Allocator* allocator, int newLength)
		{
			if (!IsCreated)
			{
				this = new List<T>(allocator, newLength);
				return true;
			}

			if (newLength <= Capacity)
			{
				return false;
			}

			return EnsureCapacity(allocator, newLength);
		}

		[INLINE(256)]
		public void AddRange<TEnumerable>(TEnumerable collection) where TEnumerable: IEnumerable<T>
		{
			foreach (var value in collection)
			{
				Add(value);
			}
		}

		[INLINE(256)]
		public void AddRange<TEnumerable>(Allocator* allocator, TEnumerable collection) where TEnumerable: IEnumerable<T>
		{
			foreach (var value in collection)
			{
				Add(allocator, value);
			}
		}

		[INLINE(256)]
		public readonly void CopyTo(Allocator* allocator, MemArray<T> arr, int srcOffset, int index, int count)
		{
			allocator->MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arr.innerArray.ptr.memPtr, index, count);
		}

		[INLINE(256)]
		public readonly void CopyTo(Allocator* allocator, in MemPtr arrPtr, int srcOffset, int index, int count)
		{
			allocator->MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arrPtr, index, count);
		}

		public int GetReservedSizeInBytes()
		{
			return _arr.GetReservedSizeInBytes();
		}

		[INLINE(256)]
		public ListEnumerator<T> GetEnumerator(Allocator* allocator)
		{
			return new ListEnumerator<T>(GetValuePtr(allocator), Count);
		}

		[INLINE(256)]
		public new ListEnumerator<T> GetEnumerator()
		{
			return new ListEnumerator<T>(GetValuePtr(), Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator GetPtrEnumerator(Allocator* allocator)
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(allocator), ElementSize, Count);
		}

		[INLINE(256)]
		public ListPtrEnumerator GetPtrEnumerator()
		{
			return new ListPtrEnumerator((byte*)GetValuePtr(), ElementSize, Count);
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetValuePtr(allocator), Count));
		}

		[INLINE(256)]
		public Enumerable<T, ListEnumerator<T>> GetEnumerable()
		{
			return new (new (GetValuePtr(), Count));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new ((byte*)GetValuePtr(allocator), ElementSize, Count));
		}

		[INLINE(256)]
		public Enumerable<IntPtr, ListPtrEnumerator> GetPtrEnumerable()
		{
			return new (new ((byte*)GetValuePtr(), ElementSize, Count));
		}

		[INLINE(256)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[INLINE(256)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
