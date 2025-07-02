using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public interface IListEnumerable<T>
		where T: unmanaged
	{
		public int Count { get; }
		public SafePtr<T> GetValuePtr(WorldState worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new ListEnumerator<T>(GetValuePtr(worldState), 0, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}

	public readonly ref struct ListEnumerable<T>
		where T : unmanaged
	{
		private readonly ListEnumerator<T> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ListEnumerable(ListEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator<T> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public ref struct ListEnumerator<T> where T: unmanaged
	{
		private readonly SafePtr<T> _valuePtr;
		private readonly int _count;

		private int _index;

		public ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref (_valuePtr + _index).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator(SafePtr<T> valuePtr, int count) : this(valuePtr, 0, count)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ListEnumerator(SafePtr<T> valuePtr, int startIndex, int count)
		{
			_valuePtr = valuePtr;
			_index = startIndex - 1;
			_count = count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			return ++_index < _count;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
