using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public interface IMemListEnumerable<T>
		where T: unmanaged
	{
		public int Count { get; }
		public SafePtr<T> GetValuePtr(WorldState worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemListEnumerator<T>(GetValuePtr(worldState), 0, Count);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}

	public readonly ref struct MemListEnumerable<T>
		where T : unmanaged
	{
		private readonly MemListEnumerator<T> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemListEnumerable(MemListEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator<T> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public ref struct MemListEnumerator<T> where T: unmanaged
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
		public MemListEnumerator(SafePtr<T> valuePtr, int count) : this(valuePtr, 0, count)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemListEnumerator(SafePtr<T> valuePtr, int startIndex, int count)
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
