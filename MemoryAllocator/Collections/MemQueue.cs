using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(MemQueue<>.QueueProxy))]
	public struct MemQueue<T> : IMemCircularBufferEnumerable<T> where T : unmanaged
	{
		private const int _minimumGrow = 4;
		private const int _growFactor = 200;

		private MemArray<T> _array;
		private int _headIndex;
		private int _tailIndex;
		private int _count;

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _array.IsCreated;
		}

		public int HeadIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _headIndex;
		}

		public readonly int Count
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _count;
		}

		public readonly int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _array.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemQueue(WorldState worldState, int capacity)
		{
			this = default;
			_array = new MemArray<T>(worldState, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(WorldState worldState)
		{
			_array.Dispose(worldState);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(WorldState worldState)
		{
			return _array.GetValuePtr(worldState);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_headIndex = 0;
			_tailIndex = 0;
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(WorldState worldState, T item)
		{
			if (_count == _array.Length)
			{
				var newCapacity = (int)((long)_array.Length * (long)_growFactor / 100L);
				if (newCapacity < _array.Length + _minimumGrow)
				{
					newCapacity = _array.Length + _minimumGrow;
				}

				SetCapacity(worldState, newCapacity);
			}

			_array[worldState, _tailIndex] = item;
			_tailIndex = (_tailIndex + 1) % _array.Length;
			_count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Dequeue(WorldState worldState)
		{
			var removed = _array[worldState, _headIndex];
			_array[worldState, _headIndex] = default;
			_headIndex = (_headIndex + 1) % _array.Length;
			_count--;
			return removed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek(WorldState worldState)
		{
			return _array[worldState, _headIndex];
		}

		public bool Contains<TU>(WorldState worldState, TU item) where TU : System.IEquatable<T>
		{
			var index = _headIndex;
			var count = _count;

			while (count-- > 0)
			{
				if (item.Equals(_array[worldState, index]))
				{
					return true;
				}

				index = (index + 1) % _array.Length;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetElement(WorldState worldState, int i)
		{
			return _array[worldState, (_headIndex + i) % _array.Length];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCapacity(WorldState worldState, int capacity)
		{
			_array.Resize(worldState, capacity);
			_headIndex = 0;
			_tailIndex = _count == capacity ? 0 : _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemCircularBufferEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemCircularBufferEnumerator<T>(GetValuePtr(worldState), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemCircularBufferEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}

		private class QueueProxy
		{
			private MemQueue<T> _queue;

			public QueueProxy(MemQueue<T> queue)
			{
				_queue = queue;
			}

			public int Count => _queue.Count;

			public T[] Items
			{
				get
				{
#if DEBUG
					var arr = new T[_queue.Count];
					var i = 0;
					var worldState = _queue._array.GetWorldState_DEBUG();
					var e = _queue.GetEnumerator(worldState);
					while (e.MoveNext())
					{
						arr[i++] = e.Current;
					}

					e.Dispose();

					return arr;
#else
					return System.Array.Empty<T>();
#endif
				}
			}
		}
	}
}
