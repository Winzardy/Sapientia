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
		public DictionaryEnumerable<TKey, TValue> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}

	public readonly ref struct DictionaryEnumerable<TKey, TValue>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		private readonly DictionaryEnumerator<TKey, TValue> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal DictionaryEnumerable(DictionaryEnumerator<TKey, TValue> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public DictionaryEnumerator<TKey, TValue> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public struct DictionaryEnumerator<TKey, TValue>
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

		public ref Dictionary<TKey, TValue>.Entry Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref (_entries + _index).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			this = default;
		}
	}
}
