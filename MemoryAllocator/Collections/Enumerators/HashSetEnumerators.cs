using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface IHashSetEnumerable<T> : IEnumerable<T>
		where T: unmanaged, IEquatable<T>
	{
		public int LastIndex { get; }
		public SafePtr<HashSet<T>.Slot> GetSlotPtr();
		public SafePtr<HashSet<T>.Slot> GetSlotPtr(SafePtr<Allocator> allocator);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerator<T> GetEnumerator(SafePtr<Allocator> allocator)
		{
			return new HashSetEnumerator<T>(GetSlotPtr(allocator), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new HashSetEnumerator<T> GetEnumerator()
		{
			return new HashSetEnumerator<T>(GetSlotPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator(SafePtr<Allocator> allocator)
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(allocator), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator()
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetSlotPtr(allocator), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable()
		{
			return new (new (GetSlotPtr(), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, HashSetPtrEnumerator<T>> GetPtrEnumerable(SafePtr<Allocator> allocator)
		{
			return new (new (GetSlotPtr(allocator), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, HashSetPtrEnumerator<T>> GetPtrEnumerable()
		{
			return new (new (GetSlotPtr(), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public unsafe struct HashSetPtrEnumerator<T> : IEnumerator<SafePtr<T>>, IEnumerator<SafePtr>
		where T: unmanaged, IEquatable<T>
	{
		private readonly SafePtr<HashSet<T>.Slot> _slots;
		private readonly int _lastIndex;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal HashSetPtrEnumerator(SafePtr<HashSet<T>.Slot> slots, int lastIndex)
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

		SafePtr IEnumerator<SafePtr>.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new SafePtr<T>(&(_slots + _index).ptr->value, 1);
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		public SafePtr<T> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new SafePtr<T>(&(_slots + _index).ptr->value, 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}

	public unsafe struct HashSetEnumerator<T> : IEnumerator<T>
		where T: unmanaged, IEquatable<T>
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

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_slots + _index).Value().value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
