using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public unsafe interface IDictionaryEnumerable<TKey, TValue>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		public int LastIndex { get; }
		public SafePtr<Dictionary<TKey, TValue>.Entry> GetEntryPtr(WorldState worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DictionaryEnumerator<TKey, TValue> GetEnumerator(WorldState worldState)
		{
			return new DictionaryEnumerator<TKey, TValue>(GetEntryPtr(worldState), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DictionaryPtrEnumerator<TKey, TValue> GetPtrEnumerator(WorldState worldState)
		{
			return new DictionaryPtrEnumerator<TKey, TValue>(GetEntryPtr(worldState), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<Dictionary<TKey, TValue>.Entry, DictionaryEnumerator<TKey, TValue>> GetEnumerable(WorldState worldState)
		{
			return new (new (GetEntryPtr(worldState), LastIndex));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Enumerable<SafePtr<TValue>, DictionaryPtrEnumerator<TKey, TValue>> GetPtrEnumerable(WorldState worldState)
		{
			return new (new (GetEntryPtr(worldState), LastIndex));
		}
	}

	public unsafe struct DictionaryPtrEnumerator<TKey, TValue> : IEnumerator<SafePtr<TValue>>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		private readonly SafePtr<Dictionary<TKey, TValue>.Entry> _entries;
		private readonly int _count;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal DictionaryPtrEnumerator(SafePtr<Dictionary<TKey, TValue>.Entry> entries, int count)
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
				if (_entries[_index].hashCode >= 0)
					return true;
			}

			_index = _count;
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

		public SafePtr<TValue> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => new SafePtr<TValue>(&(_entries + _index).ptr->value, 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}

	public unsafe struct DictionaryEnumerator<TKey, TValue> :
		IEnumerator<Dictionary<TKey, TValue>.Entry>,
		IEnumerator<SafePtr<Dictionary<TKey, TValue>.Entry>>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		private readonly SafePtr<Dictionary<TKey, TValue>.Entry> _entries;
		private readonly int _count;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal DictionaryEnumerator(SafePtr<Dictionary<TKey, TValue>.Entry> entries, int count)
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
				if (_entries[_index].hashCode >= 0)
					return true;
			}

			_index = _count;
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

		SafePtr<Dictionary<TKey, TValue>.Entry> IEnumerator<SafePtr<Dictionary<TKey, TValue>.Entry>>.Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _entries + _index;
		}

		public Dictionary<TKey, TValue>.Entry Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (_entries + _index).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
