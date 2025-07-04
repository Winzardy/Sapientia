using System;
using System.Runtime.CompilerServices;
using Sapientia.Data;

namespace Sapientia.MemoryAllocator
{
	public interface IMemDictionaryEnumerable<TKey, TValue>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		public int LastIndex { get; }
		public SafePtr<MemDictionary<TKey, TValue>.Entry> GetEntryPtr(WorldState worldState);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemDictionaryEnumerator<TKey, TValue> GetEnumerator(WorldState worldState)
		{
			return new MemDictionaryEnumerator<TKey, TValue>(GetEntryPtr(worldState), LastIndex);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemDictionaryEnumerable<TKey, TValue> GetEnumerable(WorldState worldState)
		{
			return new (GetEnumerator(worldState));
		}
	}

	public readonly ref struct MemDictionaryEnumerable<TKey, TValue>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		private readonly MemDictionaryEnumerator<TKey, TValue> _enumerator;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemDictionaryEnumerable(MemDictionaryEnumerator<TKey, TValue> enumerator)
		{
			_enumerator = enumerator;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public MemDictionaryEnumerator<TKey, TValue> GetEnumerator()
		{
			return _enumerator;
		}
	}

	public struct MemDictionaryEnumerator<TKey, TValue>
		where TKey: unmanaged, IEquatable<TKey>
		where TValue: unmanaged
	{
		private readonly SafePtr<MemDictionary<TKey, TValue>.Entry> _entries;
		private readonly int _count;
		private int _index;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal MemDictionaryEnumerator(SafePtr<MemDictionary<TKey, TValue>.Entry> entries, int count)
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

		public ref MemDictionary<TKey, TValue>.Entry Current
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
