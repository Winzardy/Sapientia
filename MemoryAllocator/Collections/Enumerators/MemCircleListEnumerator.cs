using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public readonly ref struct MemCircularBufferEnumerable<T>
		where T: unmanaged
	{
		private readonly MemCircularBufferEnumerator<T> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemCircularBufferEnumerable(MemCircularBufferEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemCircularBufferEnumerator<T> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public struct MemCircularBufferEnumerator<T>
		where T: unmanaged
	{
		private readonly SafePtr<T> _valuePtr;
		private readonly int _headIndex;
		private readonly int _count;
		private readonly int _capacity;

		private int _index;
		private int _currentCount;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemCircularBufferEnumerator(SafePtr<T> valuePtr, int headIndex, int count, int capacity)
		{
			_valuePtr = valuePtr;
			_headIndex = headIndex;
			_count = count;
			_capacity = capacity;

			_currentCount = 0;
			_index = headIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			if (_currentCount == _count)
				return false;

			_index = (_headIndex + _currentCount) % _capacity;
			_currentCount++;
			return true;
		}

		public ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref (_valuePtr + _index).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = _headIndex;
			_currentCount = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
