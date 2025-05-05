using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	[System.Diagnostics.DebuggerTypeProxyAttribute(typeof(QueueProxy<>))]
	public unsafe struct Queue<T> : ICircularBufferEnumerable<T> where T : unmanaged
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

#if UNITY_EDITOR
		internal World GetWorld()
		{
			return _array.GetWorld();
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Queue(World world, int capacity)
		{
			this = default;
			_array = new MemArray<T>(world, capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose(World world)
		{
			_array.Dispose(world);
			this = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetValuePtr(World world)
		{
			return _array.GetValuePtr(world);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			_headIndex = 0;
			_tailIndex = 0;
			_count = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(World world, T item)
		{
			if (_count == _array.Length)
			{
				var newCapacity = (int)((long)_array.Length * (long)_growFactor / 100L);
				if (newCapacity < _array.Length + _minimumGrow)
				{
					newCapacity = _array.Length + _minimumGrow;
				}

				SetCapacity(world, newCapacity);
			}

			_array[world, _tailIndex] = item;
			_tailIndex = (_tailIndex + 1) % _array.Length;
			_count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Dequeue(World world)
		{
			var removed = _array[world, _headIndex];
			_array[world, _headIndex] = default;
			_headIndex = (_headIndex + 1) % _array.Length;
			_count--;
			return removed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Peek(World world)
		{
			return _array[world, _headIndex];
		}

		public bool Contains<TU>(World world, TU item) where TU : System.IEquatable<T>
		{
			var index = _headIndex;
			var count = _count;

			while (count-- > 0)
			{
				if (item.Equals(_array[world, index]))
				{
					return true;
				}

				index = (index + 1) % _array.Length;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private T GetElement(World world, int i)
		{
			return _array[world, (_headIndex + i) % _array.Length];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void SetCapacity(World world, int capacity)
		{
			_array.Resize(world, capacity);
			_headIndex = 0;
			_tailIndex = _count == capacity ? 0 : _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferEnumerator<T> GetEnumerator(World world)
		{
			return new CircularBufferEnumerator<T>(GetValuePtr(world), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public CircularBufferPtrEnumerator<T> GetPtrEnumerator(World world)
		{
			return new CircularBufferPtrEnumerator<T>(GetValuePtr(world), HeadIndex, Count, Capacity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, CircularBufferEnumerator<T>> GetEnumerable(World world)
		{
			return new (new (GetValuePtr(world), HeadIndex, Count, Capacity));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, CircularBufferPtrEnumerator<T>> GetPtrEnumerable(World world)
		{
			return new (new (GetValuePtr(world), HeadIndex, Count, Capacity));
		}
	}
}
