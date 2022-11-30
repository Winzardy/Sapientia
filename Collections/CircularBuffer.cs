using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Sapientia.Collections
{
	public struct CircularBuffer<T> : IDisposable
	{
		private T[] _buffer;

		private int _firstData;
		private int _last;
		private int _length;

		private readonly int _expandStep;

		public int FirstBufferIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _firstData;
		}

		public int LastBufferIndex
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _last;
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
			get => _buffer[_firstData];
		}

		public T Last
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer[_last];
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

			_last = _buffer.Length - 1;
			_firstData = _length = 0;

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
				_firstData = NextPosition(_firstData);
			}
			else
			{
				_length++;
			}

			_last = NextPosition(_last);
			_buffer[_last] = toAdd;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddFirstAndExpand(T toAdd)
		{
			if (IsFull)
			{
				Expand();
			}

			AddFirst(toAdd);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddLastAndExpand(T toAdd)
		{
			if (IsFull)
			{
				Expand();
			}

			AddLast(toAdd);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddFirst(T toAdd)
		{
			if (IsFull)
			{
				_last = PreviousPosition(_last);
			}
			else
			{
				_length++;
			}

			_firstData = PreviousPosition(_firstData);
			_buffer[_firstData] = toAdd;
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
			var removed = _buffer[_firstData];
			_firstData = NextPosition(_firstData);
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
			var removed = _buffer[_last];
			_last = PreviousPosition(_last);
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

		public void Expand()
		{
			var newLength = _buffer.Length + _expandStep;

			var newArr = ArrayPool<T>.Shared.Rent(newLength);
			CopyAndOrderData(newArr);

			ArrayPool<T>.Shared.Return(_buffer);

			_buffer = newArr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void CopyAndOrderData(T[] to)
		{
			if (_firstData > _last)
			{
				var firstToEnd = _buffer.Length - _firstData;
				Array.Copy(_buffer, _firstData, to, 0, firstToEnd);
				Array.Copy(_buffer, 0, to, firstToEnd, _last + 1);
			}
			else
			{
				Array.Copy(_buffer, _firstData, to, 0, Length);
			}

			_firstData = 0;
			_last = Length - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private int BufferIndex(int index)
		{
			return (_firstData + index) % _buffer.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			ArrayPool<T>.Shared.Return(_buffer);
		}
	}
}