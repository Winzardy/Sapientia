using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public interface IMemHashSetEnumerable<T>
		where T: unmanaged, IEquatable<T>
	{
		public int LastIndex { get; }
		public SafePtr<MemHashSet<T>.Slot> GetSlotPtr(WorldState worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSetEnumerator<T> GetEnumerator(WorldState worldState)
		{
			return new MemHashSetEnumerator<T>(GetSlotPtr(worldState), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSetEnumerable<T> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}

	public readonly ref struct MemHashSetEnumerable<T>
		where T : unmanaged, IEquatable<T>
	{
		private readonly MemHashSetEnumerator<T> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemHashSetEnumerable(MemHashSetEnumerator<T> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemHashSetEnumerator<T> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public ref struct MemHashSetEnumerator<T> where T: unmanaged, IEquatable<T>
	{
		private readonly SafePtr<MemHashSet<T>.Slot> _slots;
		private readonly int _lastIndex;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemHashSetEnumerator(SafePtr<MemHashSet<T>.Slot> slots, int lastIndex)
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
