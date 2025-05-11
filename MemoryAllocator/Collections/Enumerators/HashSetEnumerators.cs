using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface IHashSetEnumerable<T>
		where T: unmanaged, IEquatable<T>
	{
		public int LastIndex { get; }
		public SafePtr<HashSet<T>.Slot> GetSlotPtr(World world);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerator<T> GetEnumerator(World world)
		{
			return new HashSetEnumerator<T>(GetSlotPtr(world), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator(World world)
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(world), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable(World world)
		{
			return new (new (GetSlotPtr(world), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<T>, HashSetPtrEnumerator<T>> GetPtrEnumerable(World world)
		{
			return new (new (GetSlotPtr(world), LastIndex));
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
