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
		public Stack(WorldState worldState, int capacity)
		{
			this = default;
			_array = new MemArray<T>(worldState, capacity);
		}

		[INLINE(256)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			return _array.GetValuePtr(worldState);
		}

		[INLINE(256)]
		public void Dispose(WorldState worldState)
		{
			_array.Dispose(worldState);
			this = default;
		}

		[INLINE(256)]
		public void Clear()
		{
			_count = 0;
		}

		[INLINE(256)]
		public bool Contains<TU>(WorldState worldState, TU item) where TU : IEquatable<T>
		{
			var count = _count;
			while (count-- > 0)
			{
				if (item.Equals(_array[worldState, count]))
				{
					return true;
				}
			}

			return false;
		}

		[INLINE(256)]
		public readonly T Peek(WorldState worldState)
		{
			return _array[worldState, _count - 1];
		}

		[INLINE(256)]
		public T Pop(WorldState worldState)
		{
			var item = _array[worldState, --_count];
			_array[worldState, _count] = default;
			return item;
		}

		[INLINE(256)]
		public void Push(WorldState worldState, T item)
		{
			if (_count == _array.Length)
			{
				_array.Resize(worldState, _array.Length == 0 ? _defaultCapacity : 2 * _array.Length);
			}

			_array[worldState, _count++] = item;
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
		public ListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new ListEnumerator<T>(GetValuePtr(worldState), Count);
		}

		[INLINE(256)]
		public ListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
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
					var worldState = _stack._array.GetWorldState_DEBUG();
					var arr = new T[_stack.Count];
					var i = 0;
					var e = _stack.GetEnumerator(worldState);
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
