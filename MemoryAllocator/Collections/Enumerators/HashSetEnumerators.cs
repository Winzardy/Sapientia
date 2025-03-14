using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface IHashSetEnumerable<T> : IEnumerable<T>
		where T: unmanaged
	{
		public int LastIndex { get; }
		public HashSet<T>.Slot* GetSlotPtr();
		public HashSet<T>.Slot* GetSlotPtr(Allocator* allocator);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetEnumerator<T> GetEnumerator(Allocator* allocator)
		{
			return new HashSetEnumerator<T>(GetSlotPtr(allocator), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new HashSetEnumerator<T> GetEnumerator()
		{
			return new HashSetEnumerator<T>(GetSlotPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator(Allocator* allocator)
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(allocator), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public HashSetPtrEnumerator<T> GetPtrEnumerator()
		{
			return new HashSetPtrEnumerator<T>(GetSlotPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetSlotPtr(allocator), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<T, HashSetEnumerator<T>> GetEnumerable()
		{
			return new (new (GetSlotPtr(), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<T>> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new (GetSlotPtr(allocator), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, HashSetPtrEnumerator<T>> GetPtrEnumerable()
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

	public unsafe struct HashSetPtrEnumerator<T> : IEnumerator<IntPtr>
		where T: unmanaged
	{
		private readonly HashSet<T>.Slot* _slots;
		private readonly int _lastIndex;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal HashSetPtrEnumerator(HashSet<T>.Slot* slots, int lastIndex)
		{
			_slots = slots;
			_lastIndex = lastIndex;
			_index = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (_index < _lastIndex)
			{
				if (_slots[_index++].hashCode >= 0)
					return true;
			}
			_index = _lastIndex + 1;

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = 0;
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		public IntPtr Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (IntPtr)(&(_slots + _index - 1)->value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}

	public unsafe struct HashSetEnumerator<T> : IEnumerator<T>
		where T: unmanaged
	{
		private readonly HashSet<T>.Slot* _slots;
		private readonly int _lastIndex;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal HashSetEnumerator(HashSet<T>.Slot* slots, int lastIndex)
		{
			_slots = slots;
			_lastIndex = lastIndex;
			_index = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (_index < _lastIndex)
			{
				if (_slots[_index++].hashCode >= 0)
					return true;
			}

			_index = _lastIndex + 1;
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reset()
		{
			_index = 0;
		}

		object IEnumerator.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Current;
		}

		public T Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_slots + _index - 1)->value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
