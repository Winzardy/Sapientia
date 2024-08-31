using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxyAttribute(typeof(ListProxy<>))]
	public unsafe struct List<T> : IIsCreated, IEnumerable<T> where T : unmanaged
	{
		public struct IntPtrEnumerable : IEnumerable<IntPtr>
		{
			private readonly List<T>* _list;
			private readonly Allocator* _allocator;

			[INLINE(256)]
			internal IntPtrEnumerable(Allocator* allocator, List<T>* list)
			{
				_list = list;
				_allocator = allocator;
			}

			[INLINE(256)]
			public IEnumerator<IntPtr> GetEnumerator()
			{
				return new IntPtrEnumerator(_allocator, _list);
			}

			[INLINE(256)]
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		public struct IntPtrEnumerator : IEnumerator<IntPtr>
		{
			private readonly List<T>* _list;
			private readonly Allocator* _allocator;
			private uint _index;

			[INLINE(256)]
			internal IntPtrEnumerator(Allocator* allocator, List<T>* list)
			{
				_list = list;
				_allocator = allocator;
				_index = 0u;
			}

			[INLINE(256)]
			public bool MoveNext()
			{
				return _index++ < _list->count;
			}

			[INLINE(256)]
			public void Reset()
			{
				_index = 0u;
			}

			public IntPtr Current
			{
				[INLINE(256)]
				get => (IntPtr)(*_list).GetValuePtr(in *_allocator, _index - 1u);
			}

			object IEnumerator.Current
			{
				[INLINE(256)]
				get => (*_list)[in *_allocator, _index - 1u];
			}

			[INLINE(256)]
			public void Dispose()
			{
				this = default;
			}
		}

		public struct Enumerator : IEnumerator<T>
		{
			private readonly List<T>* _list;
			private readonly Allocator* _allocator;
			private uint _index;

			[INLINE(256)]
			internal Enumerator(Allocator* allocator, List<T>* list)
			{
				_list = list;
				_allocator = allocator;
				_index = 0u;
			}

			[INLINE(256)]
			public bool MoveNext()
			{
				return _index++ < _list->count;
			}

			[INLINE(256)]
			public void Reset()
			{
				_index = 0u;
			}

			public T Current
			{
				[INLINE(256)]
				get => (*_list)[in *_allocator, _index - 1u];
			}

			object IEnumerator.Current
			{
				[INLINE(256)]
				get => (*_list)[in *_allocator, _index - 1u];
			}

			[INLINE(256)]
			public void Dispose()
			{
				this = default;
			}
		}

		private MemArray<T> _arr;
		public uint count;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _arr.IsCreated;
		}

		public uint Capacity
		{
			[INLINE(256)]
			get => _arr.Length;
		}

		[INLINE(256)]
		public ref Allocator GetAllocator()
		{
			return ref _arr.GetAllocator();
		}

		[INLINE(256)]
		public List(uint capacity = 8) : this(ref AllocatorManager.CurrentAllocator, capacity)
		{

		}

		[INLINE(256)]
		public List(ref Allocator allocator, uint capacity = 8)
		{
			Debug.Assert(capacity > 0u);

			this = default;
			EnsureCapacity(ref allocator, capacity);
		}

		[INLINE(256)]
		public void BurstMode(in Allocator allocator, bool state)
		{
			_arr.BurstMode(in allocator, state);
		}

		[INLINE(256)]
		public void ReplaceWith(ref Allocator allocator, in List<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
			{
				return;
			}

			Dispose(ref allocator);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(ref Allocator allocator, in List<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
				return;
			if (!_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
				return;
			if (_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
			{
				Dispose(ref allocator);
				return;
			}

			if (!_arr.innerArray.ptr.memPtr.IsValid())
				this = new List<T>(ref allocator, other.Capacity);

			MemArrayExt.Copy(ref allocator, in other._arr.innerArray, ref _arr.innerArray);
			count = other.count;
		}

		[INLINE(256)]
		public readonly MemPtr GetMemPtr()
		{
			return _arr.innerArray.ptr.memPtr;
		}

		[INLINE(256)]
		public void* GetUnsafePtr(in Allocator allocator)
		{
			return _arr.innerArray.GetPtr(allocator);
		}

		[INLINE(256)]
		public void Dispose(ref Allocator allocator)
		{
			_arr.Dispose(ref allocator);
			this = default;
		}

		[INLINE(256)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		[INLINE(256)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[INLINE(256)]
		public Enumerator GetEnumerator()
		{
			Debug.Assert(IsCreated);
			return new Enumerator(_arr.innerArray.ptr.GetAllocatorPtr(), (List<T>*)this.AsPointer());
		}

		[INLINE(256)]
		public Enumerator GetEnumerator(ref Allocator allocator)
		{
			Debug.Assert(IsCreated);
			return new Enumerator((Allocator*)allocator.AsPointer(), (List<T>*)this.AsPointer());
		}

		[INLINE(256)]
		public IntPtrEnumerator GetIntPtrEnumerator()
		{
			return new IntPtrEnumerator(_arr.innerArray.ptr.GetAllocatorPtr(), (List<T>*)this.AsPointer());
		}

		[INLINE(256)]
		public IntPtrEnumerator GetIntPtrEnumerator(ref Allocator allocator)
		{
			return new IntPtrEnumerator((Allocator*)allocator.AsPointer(), (List<T>*)this.AsPointer());
		}

		[INLINE(256)]
		public IntPtrEnumerable GetIntPtrEnumerable(ref Allocator allocator)
		{
			return new IntPtrEnumerable((Allocator*)allocator.AsPointer(), (List<T>*)this.AsPointer());
		}

		[INLINE(256)]
		public IntPtrEnumerable GetIntPtrEnumerable()
		{
			return new IntPtrEnumerable(_arr.innerArray.ptr.GetAllocatorPtr(), (List<T>*)this.AsPointer());
		}

		[INLINE(256)]
		public void Clear()
		{
			count = 0;
		}

		public ref T this[uint index]
		{
			[INLINE(256)]
			get => ref _arr[GetAllocator(), index];
		}

		public ref T this[ushort index]
		{
			[INLINE(256)]
			get => ref _arr[GetAllocator(), index];
		}

		public ref T this[in Allocator allocator, uint index]
		{
			[INLINE(256)]
			get
			{
				Debug.Assert(index < count);
				return ref _arr[in allocator, index];
			}
		}

		public ref T this[in Allocator allocator, ushort index]
		{
			[INLINE(256)]
			get
			{
				Debug.Assert(index < count);
				return ref _arr[in allocator, index];
			}
		}

		[INLINE(256)]
		public T* GetValuePtr(in Allocator allocator, uint index)
		{
			return _arr.GetValuePtr(allocator, index);
		}

		[INLINE(256)]
		public bool EnsureCapacity(uint capacity)
		{
			return EnsureCapacity(ref GetAllocator(), capacity);
		}

		[INLINE(256)]
		public bool EnsureCapacity(ref Allocator allocator, uint capacity)
		{
			capacity = Helpers.NextPot(capacity);
			if (_arr.IsCreated == false)
				_arr.innerArray.growFactor = 1;
			return _arr.Resize(ref allocator, capacity, ClearOptions.UninitializedMemory);
		}

		[INLINE(256)]
		public uint Add(T obj)
		{
			Debug.Assert(IsCreated);

			ref var allocator = ref _arr.innerArray.ptr.GetAllocator();
			++count;
			EnsureCapacity(ref allocator, count);

			_arr[in allocator, count - 1u] = obj;
			return count - 1u;
		}

		[INLINE(256)]
		public uint Add(ref Allocator allocator, T obj)
		{
			Debug.Assert(IsCreated);
			++count;
			EnsureCapacity(ref allocator, count);

			_arr[in allocator, count - 1u] = obj;
			return count - 1u;
		}

		[INLINE(256)]
		public readonly bool Contains<TU>(in Allocator allocator, TU obj) where TU : unmanaged, IEquatable<T>
		{
			Debug.Assert(IsCreated);
			for (uint i = 0, cnt = count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[in allocator, i]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool Remove<TU>(ref Allocator allocator, TU obj) where TU : unmanaged, IEquatable<T>
		{
			Debug.Assert(IsCreated);
			for (uint i = 0, cnt = count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[in allocator, i]))
				{
					RemoveAt(ref allocator, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveFast<TU>(in Allocator allocator, TU obj) where TU : unmanaged, IEquatable<T>
		{
			for (uint i = 0, cnt = count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[in allocator, i]))
				{
					RemoveAtFast(in allocator, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveAt(ref Allocator allocator, uint index)
		{
			if (index >= count) return false;

			if (index == count - 1)
			{
				--count;
				_arr[in allocator, count] = default;
				return true;
			}

			var ptr = _arr.innerArray.ptr.memPtr;
			var size = sizeof(T);
			allocator.MemMove(ptr, size * index, ptr, size * (index + 1), (count - index - 1) * size);

			--count;
			_arr[in allocator, count] = default;

			return true;
		}

		[INLINE(256)]
		public bool RemoveAtFast(in Allocator allocator, uint index)
		{
			if (index >= count)
				return false;

			--count;
			var last = _arr[in allocator, count];
			_arr[in allocator, index] = last;

			return true;
		}

		[INLINE(256)]
		public bool Resize(ref Allocator allocator, uint newLength)
		{
			if (!IsCreated)
			{
				this = new List<T>(ref allocator, newLength);
				return true;
			}

			if (newLength <= Capacity)
			{
				return false;
			}

			return EnsureCapacity(ref allocator, newLength);
		}

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, in List<T> collection)
		{
			AddRange(ref allocator, in collection, 0u, collection.count);
		}

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, in List<T> collection, uint fromIdx, uint toIdx)
		{
			var index = count;

			var srcOffset = fromIdx;
			var length = toIdx - fromIdx;
			if (length > 0u)
			{
				EnsureCapacity(ref allocator, count + length);
				var size = sizeof(T);
				if (index < count)
				{
					allocator.MemMove(_arr.innerArray.ptr.memPtr, (index + length) * size, _arr.innerArray.ptr.memPtr, index * size,
						(count - index) * size);
				}

				if (_arr.innerArray.ptr.memPtr == collection._arr.innerArray.ptr.memPtr)
				{
					allocator.MemMove(_arr.innerArray.ptr.memPtr, index * size, _arr.innerArray.ptr.memPtr, 0, index * size);
					allocator.MemMove(_arr.innerArray.ptr.memPtr, (index * 2) * size, _arr.innerArray.ptr.memPtr, (index + length) * size,
						(count - index) * size);
				}
				else
				{
					collection.CopyTo(ref allocator, _arr, srcOffset, index, length);
				}

				count += length;
			}
		}

		[INLINE(256)]
		public void AddRange(ref Allocator allocator, MemArray<T> collection)
		{
			var index = count;
			var length = collection.Length;
			if (length > 0u)
			{
				EnsureCapacity(ref allocator, count + length);
				var size = sizeof(T);
				if (index < count)
				{
					allocator.MemMove(_arr.innerArray.ptr.memPtr, (index + length) * size, _arr.innerArray.ptr.memPtr, index * size,
						(count - index) * size);
				}

				if (_arr.innerArray.ptr.memPtr == collection.innerArray.ptr.memPtr)
				{
					allocator.MemMove(_arr.innerArray.ptr.memPtr, index * size, _arr.innerArray.ptr.memPtr, 0, index * size);
					allocator.MemMove(_arr.innerArray.ptr.memPtr, (index * 2) * size, _arr.innerArray.ptr.memPtr, (index + length) * size,
						(count - index) * size);
				}
				else
				{
					CopyFrom(ref allocator, collection, index);
				}

				count += length;
			}
		}

		[INLINE(256)]
		public readonly void CopyTo(ref Allocator allocator, MemArray<T> arr, uint srcOffset, uint index, uint count)
		{
			var size = sizeof(T);
			allocator.MemCopy(arr.innerArray.ptr.memPtr, index * size, _arr.innerArray.ptr.memPtr, srcOffset * size, count * size);
		}

		[INLINE(256)]
		public readonly void CopyTo(ref Allocator allocator, in MemPtr arrPtr, uint srcOffset, uint index, uint count)
		{
			var size = sizeof(T);
			allocator.MemCopy(arrPtr, index * size, _arr.innerArray.ptr.memPtr, srcOffset * size, count * size);
		}

		[INLINE(256)]
		public readonly void CopyFrom(ref Allocator allocator, MemArray<T> arr, uint index)
		{
			var size = sizeof(T);
			allocator.MemCopy(_arr.innerArray.ptr.memPtr, index * size, arr.innerArray.ptr.memPtr, 0, arr.Length * size);
		}

		public uint GetReservedSizeInBytes()
		{
			return _arr.GetReservedSizeInBytes();
		}
	}
}
