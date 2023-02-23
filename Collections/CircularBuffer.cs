using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections
{
	public struct CircularBuffer<T> : IDisposable
	{
		private T[] _buffer;

		private int _firstIndex;
		private int _lastIndex;
		private int _length;

		private readonly int _expandStep;

		public int FirstBufferIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _firstIndex;
		}

		public int LastBufferIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _lastIndex;
		}

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _length;
		}

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer.Length;
		}

		public T First
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer[_firstIndex];
		}

		public T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer[_lastIndex];
		}

		public bool IsEmpty
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Length == 0;
		}

		public bool IsFull
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Length == _buffer.Length;
		}

		public CircularBuffer(int bufferLength) : this(bufferLength, bufferLength)
		{
		}

		public CircularBuffer(int bufferLength, int expandStep)
		{
			_buffer = ArrayPool<T>.Shared.Rent(bufferLength);

			_lastIndex = _buffer.Length - 1;
			_firstIndex = _length = 0;

			_expandStep = expandStep;
		}

		public ref T this[int i]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var index = BufferIndex(i);
				return ref _buffer[index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddLast(T toAdd)
		{
			if (IsFull)
			{
				_firstIndex = NextPosition(_firstIndex);
			}
			else
			{
				_length++;
			}

			_lastIndex = NextPosition(_lastIndex);
			_buffer[_lastIndex] = toAdd;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddFirstAndExpand(T toAdd)
		{
			if (IsFull)
			{
				ExpandCapacity();
			}

			AddFirst(toAdd);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddLastAndExpand(T toAdd)
		{
			if (IsFull)
			{
				ExpandCapacity();
			}

			AddLast(toAdd);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddFirst(T toAdd)
		{
			if (IsFull)
			{
				_lastIndex = PreviousPosition(_lastIndex);
			}
			else
			{
				_length++;
			}

			_firstIndex = PreviousPosition(_firstIndex);
			_buffer[_firstIndex] = toAdd;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRemoveFirst(out T value)
		{
			if (IsEmpty)
			{
				value = default!;
				return false;
			}

			value = RemoveFirst();
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveFirst()
		{
			var removed = _buffer[_firstIndex];
			_firstIndex = NextPosition(_firstIndex);
			_length--;
			return removed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryRemoveLast(out T value)
		{
			if (IsEmpty)
			{
				value = default!;
				return false;
			}

			value = RemoveLast();
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T RemoveLast()
		{
			var removed = _buffer[_lastIndex];
			_lastIndex = PreviousPosition(_lastIndex);
			_length--;
			return removed;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int NextPosition(int position)
		{
			var nextPosition = position + 1;
			return nextPosition == _buffer.Length ? 0 : nextPosition;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int PreviousPosition(int position)
		{
			return (position == 0 ? _buffer.Length : position) - 1;
		}

		private void ExpandCapacity()
		{
			var newCapacity = _buffer.Length + _expandStep;

			var newArr = ArrayPool<T>.Shared.Rent(newCapacity);
			CopyAndOrderData(newArr);

			ArrayPool<T>.Shared.Return(_buffer);

			_buffer = newArr;
		}

		public void ExpandCapacity(int newCapacity)
		{
			if (newCapacity <= _buffer.Length)
				return;

			var newArr = ArrayPool<T>.Shared.Rent(newCapacity);
			CopyAndOrderData(newArr);

			ArrayPool<T>.Shared.Return(_buffer);

			_buffer = newArr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyAndOrderData(T[] to)
		{
			if (_firstIndex > _lastIndex)
			{
				var firstToEnd = _buffer.Length - _firstIndex;
				Array.Copy(_buffer, _firstIndex, to, 0, firstToEnd);
				Array.Copy(_buffer, 0, to, firstToEnd, _lastIndex + 1);
			}
			else
			{
				Array.Copy(_buffer, _firstIndex, to, 0, Length);
			}

			_firstIndex = 0;
			_lastIndex = Length - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int BufferIndex(int index)
		{
			return (_firstIndex + index) % _buffer.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			ArrayPool<T>.Shared.Return(_buffer);
		}
	}
}