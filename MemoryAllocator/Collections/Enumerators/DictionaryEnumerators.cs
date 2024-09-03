using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface IDictionaryEnumerable<TKey, TValue> : IEnumerable<Dictionary<TKey, TValue>.Entry>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		public int LastIndex { get; }
		public Dictionary<TKey, TValue>.Entry* GetEntryPtr();
		public Dictionary<TKey, TValue>.Entry* GetEntryPtr(Allocator* allocator);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DictionaryEnumerator<TKey, TValue> GetEnumerator(Allocator* allocator)
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(allocator), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public new DictionaryEnumerator<TKey, TValue> GetEnumerator()
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator(Allocator* allocator)
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(allocator), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator()
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<Dictionary<TKey, TValue>.Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable(Allocator* allocator)
		{
			return new (new (GetEntryPtr(allocator), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<Dictionary<TKey, TValue>.Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable()
		{
			return new (new (GetEntryPtr(), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable(Allocator* allocator)
		{
			return new (new (GetEntryPtr(allocator), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<IntPtr, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable()
		{
			return new (new (GetEntryPtr(), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator<Dictionary<TKey, TValue>.Entry> IEnumerable<Dictionary<TKey, TValue>.Entry>.GetEnumerator()
		{
			return GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public unsafe struct DictionaryPtrEnumerator<TKey, TValue> : IEnumerator<IntPtr>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		private readonly Dictionary<TKey, TValue>.Entry* _entries;
		private readonly int _count;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal DictionaryPtrEnumerator(Dictionary<TKey, TValue>.Entry* entries, int count)
		{
			_entries = entries;
			_count = count;
			_index = -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (++_index < _count)
			{
				ref var local = ref _entries[_index];
				if (local.hashCode >= 0)
					return true;
			}

			_index = _count;
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
			get => (IntPtr)(_entries + _index);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}

	public unsafe struct DictionaryEnumerator<TKey, TValue> : IEnumerator<Dictionary<TKey, TValue>.Entry>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		private readonly Dictionary<TKey, TValue>.Entry* _entries;
		private readonly int _lastIndex;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal DictionaryEnumerator(Dictionary<TKey, TValue>.Entry* entries, int lastIndex)
		{
			_entries = entries;
			_lastIndex = lastIndex;
			_index = 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			while (_index < _lastIndex)
			{
				ref var local = ref _entries[_index++];
				if (local.hashCode >= 0)
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

		public Dictionary<TKey, TValue>.Entry Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => *(_entries + _index - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
