using System;
using System.Collections.Generic;
using System.Diagnostics;
using Sapientia.Data;
using Sapientia.Extensions;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[DebuggerTypeProxy(typeof(ListProxy<>))]
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
		public List(int capacity = 0) : this(WorldManager.CurrentWorld, capacity)
		{

		}

		[INLINE(256)]
		public List(World world, int capacity = 0)
		{
			this = default;
			EnsureCapacity(world, capacity);

			if (!_arr.IsCreated)
				_arr = new MemArray<T>(world, capacity);
		}

		[INLINE(256)]
		public List(World world, IEnumerable<T> enumerable, int capacity) : this(world, capacity)
		{
			AddRange(world, enumerable);
		}

#if UNITY_EDITOR
		internal World GetWorld()
		{
			return _arr.GetWorld();
		}
#endif

		[INLINE(256)]
		public void ReplaceWith(World world, in List<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
			{
				return;
			}

			Dispose(world);
			this = other;
		}

		[INLINE(256)]
		public void CopyFrom(World world, in List<T> other)
		{
			if (other._arr.innerArray.ptr.memPtr == _arr.innerArray.ptr.memPtr)
				return;
			if (!_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
				return;
			if (_arr.innerArray.ptr.memPtr.IsValid() && !other._arr.innerArray.ptr.memPtr.IsValid())
			{
				Dispose(world);
				return;
			}

			if (!_arr.innerArray.ptr.memPtr.IsValid())
				this = new List<T>(world, other.Capacity);

			MemArrayExt.Copy(world, in other._arr.innerArray, ref _arr.innerArray);
			_count = other._count;
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(World world)
		{
			return _arr.GetValuePtr(world);
		}

		[INLINE(256)]
		public Span<T> GetSpan(World world)
		{
			var span = _arr.GetSpan(world);
			return span[.._count];
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			_arr.Dispose(world);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			_count = 0;
		}

		public ref T this[World world, int index]
		{
			[INLINE(256)]
			get => ref _arr[world, index];
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(World world, int index)
		{
			return _arr.GetValuePtr(world, index);
		}

		[INLINE(256)]
		public bool EnsureCapacity(World world, int capacity)
		{
			if (capacity <= Capacity)
				return false;
			capacity = capacity.NextPowerOfTwo();
			return _arr.Resize(world, capacity, ClearOptions.UninitializedMemory);
		}

		[INLINE(256)]
		public void EnsureCount(World world, int count, in T defaultValue = default)
		{
			EnsureCapacity(world, count);
			if (_count < count)
			{
				_arr.Fill(world, defaultValue, _count, count - _count);
				_count = count;
			}
		}

		[INLINE(256)]
		public void SetCount(World world, int count)
		{
			EnsureCount(world, count);
			_count = count;
		}

		[INLINE(256)]
		public void SetCountNoCheck(int count)
		{
			_count = count;
		}

		[INLINE(256)]
		public void Add(World world, in T value)
		{
			EnsureCapacity(world, _count + 1);

			E.ASSERT(IsCreated);
			_arr[world, _count] = value;
			_count++;
		}

		public void Insert(World world, int index, T value)
		{
			EnsureCapacity(world, _count + 1);

			var source = GetValuePtr(world, index);

			if (_count > index)
			{
				var destination = source + 1;
				MemoryExt.MemMove<T>(source, destination, _count - index);
			}

			source.Value() = value;
			_count++;
		}

		[INLINE(256)]
		public readonly bool Contains<TU>(World world, TU obj) where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[world, i]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool Remove<TU>(World world, TU obj) where TU : unmanaged, IEquatable<T>
		{
			E.ASSERT(IsCreated);
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[world, i]))
				{
					RemoveAt(world, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveFast<TU>(World world, TU obj) where TU : unmanaged, IEquatable<T>
		{
			for (int i = 0, cnt = _count; i < cnt; ++i)
			{
				if (obj.Equals(_arr[world, i]))
				{
					RemoveAtSwapBack(world, i);
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public bool RemoveAt(World world, int index)
		{
			if (index >= _count)
				return false;

			if (index == _count - 1)
			{
				--_count;
				_arr[world, _count] = default;
				return true;
			}

			var ptr = _arr.innerArray.ptr.memPtr;
			world.MemMove<T>(ptr, index + 1, ptr, index, (_count - index - 1));

			--_count;
			_arr[world, _count] = default;

			return true;
		}

		[INLINE(256)]
		public bool RemoveAtSwapBack(World world, int index)
		{
			if (index >= _count)
				return false;

			--_count;
			var last = _arr[world, _count];
			_arr[world, index] = last;

			return true;
		}

		[INLINE(256)]
		public bool Resize(World world, int newLength)
		{
			if (!IsCreated)
			{
				this = new List<T>(world, newLength);
				return true;
			}

			if (newLength <= Capacity)
			{
				return false;
			}

			return EnsureCapacity(world, newLength);
		}

		[INLINE(256)]
		public void AddRange<TEnumerable>(World world, TEnumerable collection) where TEnumerable: IEnumerable<T>
		{
			foreach (var value in collection)
			{
				Add(world, value);
			}
		}

		[INLINE(256)]
		public readonly void CopyTo(World world, MemArray<T> arr, int srcOffset, int index, int count)
		{
			world.MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arr.innerArray.ptr.memPtr, index, count);
		}

		[INLINE(256)]
		public readonly void CopyTo(World world, in MemPtr arrPtr, int srcOffset, int index, int count)
		{
			world.MemCopy<T>(_arr.innerArray.ptr.memPtr, srcOffset, arrPtr, index, count);
		}

		public int GetReservedSizeInBytes()
		{
			return _arr.GetReservedSizeInBytes();
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
	}
}
