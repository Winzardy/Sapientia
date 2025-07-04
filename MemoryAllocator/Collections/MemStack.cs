using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemStack<>.StackProxy))]
	public unsafe struct MemStack<T> : IMemListEnumerable<T> where T : unmanaged
	{
		private const int _defaultCapacity = 4;

		private MemArray<T> _array;
		private int _count;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _array.IsCreated;
		}

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _count;
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _array.Length;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)] get => _array.Length <= _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemStack(WorldState worldState, int capacity)
		{
			this = default;
			_array = new MemArray<T>(worldState, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			return _array.GetValuePtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_array.Dispose(worldState);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly T Peek(WorldState worldState)
		{
			return _array[worldState, _count - 1];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Pop(WorldState worldState)
		{
			var item = _array[worldState, --_count];
			_array[worldState, _count] = default;
			return item;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Push(WorldState worldState, T item)
		{
			if (_count == _array.Length)
			{
				_array.Resize(worldState, _array.Length == 0 ? _defaultCapacity : 2 * _array.Length);
			}

			_array[worldState, _count++] = item;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void PushNoChecks(T item, T* ptr)
		{
			*ptr = item;
			++_count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetReservedSizeInBytes()
		{
			return _array.GetReservedSizeInBytes();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemListEnumerator<T>(GetValuePtr(worldState), Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}

		private class StackProxy
		{
			private MemStack<T> _stack;

			public StackProxy(MemStack<T> stack)
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
