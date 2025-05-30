using System;
using Sapientia.Data;
using INLINE = System.Runtime.CompilerServices.MethodImplAttribute;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(Stack<>.StackProxy))]
	public unsafe struct Stack<T> : IListEnumerable<T> where T : unmanaged
	{
		private const int _defaultCapacity = 4;

		private MemArray<T> _array;
		private int _count;

		public readonly bool IsCreated
		{
			[INLINE(256)] get => _array.IsCreated;
		}

		public readonly int Count
		{
			[INLINE(256)] get => _count;
		}

		public readonly int Capacity
		{
			[INLINE(256)] get => _array.Length;
		}

		public bool IsFull
		{
			[INLINE(256)] get => _array.Length <= _count;
		}

		[INLINE(256)]
		public Stack(World world, int capacity)
		{
			this = default;
			_array = new MemArray<T>(world, capacity);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(World world)
		{
			return _array.GetValuePtr(world);
		}

		[INLINE(256)]
		public void Dispose(World world)
		{
			_array.Dispose(world);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			_count = 0;
		}

		[INLINE(256)]
		public bool Contains<TU>(World world, TU item) where TU : IEquatable<T>
		{
			var count = _count;
			while (count-- > 0)
			{
				if (item.Equals(_array[world, count]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public readonly T Peek(World world)
		{
			return _array[world, _count - 1];
		}

		[INLINE(256)]
		public T Pop(World world)
		{
			var item = _array[world, --_count];
			_array[world, _count] = default;
			return item;
		}

		[INLINE(256)]
		public void Push(World world, T item)
		{
			if (_count == _array.Length)
			{
				_array.Resize(world, _array.Length == 0 ? _defaultCapacity : 2 * _array.Length);
			}

			_array[world, _count++] = item;
		}

		[INLINE(256)]
		public void PushNoChecks(T item, T* ptr)
		{
			*ptr = item;
			++_count;
		}

		[INLINE(256)]
		public int GetReservedSizeInBytes()
		{
			return _array.GetReservedSizeInBytes();
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

		private class StackProxy
		{
			private Stack<T> _stack;

			public StackProxy(Stack<T> stack)
			{
				_stack = stack;
			}

			public int Count => _stack.Count;

			public T[] Items
			{
				get
				{
#if DEBUG
					var allocator = _stack._array.GetWorld_DEBUG();
					var arr = new T[_stack.Count];
					var i = 0;
					var e = _stack.GetEnumerator(allocator);
					while (e.MoveNext())
					{
						arr[i++] = e.Current;
					}

					e.Dispose();

					return arr;
#else
					return Array.Empty<T>();
#endif
				}
			}
		}
	}
}
