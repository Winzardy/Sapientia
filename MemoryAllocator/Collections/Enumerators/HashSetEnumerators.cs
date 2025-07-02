using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public interface IHashSetEnumerable<T>
		where T: unmanaged, IEquatable<T>
	{
		public int LastIndex { get; }
		public SafePtr<HashSet<T>.Slot> GetSlotPtr(WorldState worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new HashSetEnumerator<T>(GetSlotPtr(worldState), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}

	public readonly ref struct HashSetEnumerable<T>
		where T : unmanaged, IEquatable<T>
	{
		private readonly HashSetEnumerator<T> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal HashSetEnumerable(HashSetEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerator<T> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public ref struct HashSetEnumerator<T> where T: unmanaged, IEquatable<T>
	{
		private readonly SafePtr<HashSet<T>.Slot> _slots;
		private readonly int _lastIndex;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal HashSetEnumerator(SafePtr<HashSet<T>.Slot> slots, int lastIndex)
		{
			_slots = slots;
			_lastIndex = lastIndex;
			_index = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (++_index < _lastIndex)
			{
				if (_slots[_index].hashCode >= 0)
					return true;
			}
			_index = _lastIndex;

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = -1;
		}

		public ref T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref (_slots + _index).Value().value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
