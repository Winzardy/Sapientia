using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia;
using Sapientia.Collections;

namespace Content.Management
{
	public sealed class ContentDictionary<TValue>
		where TValue : IUniqueContentEntry
	{
		private Dictionary<SerializableGuid, TValue> _temporary;

		//Можно будет переделать на FrozenDictionary, когда его завезут в Unity
		private Dictionary<SerializableGuid, int> _keyToIndex;

		//Можно будет переделать на FrozenDictionary, когда его завезут в Unity
		private Dictionary<string, int> _idToIndex;

		private TValue[] _values;

		public ref readonly TValue this[in SerializableGuid key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[_keyToIndex[key]];
		}

		public ref readonly TValue this[string id]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[_idToIndex[id]];
		}

		public ref readonly TValue this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _values[index];
		}

		public int Count => _values.Length;
		public int Length => _values.Length;

		public TValue[] Values => _values;

		public bool IsEmpty => _values == null || _values.Length == 0;

		public bool IsFrozen => _values != null;
		public bool IsBuilding => _temporary != null;

		public ContentDictionary()
		{
			_temporary = new Dictionary<SerializableGuid, TValue>();
		}

		public ContentDictionary(Dictionary<SerializableGuid, TValue> source)
		{
			Freeze(source);
		}

		internal void Freeze()
		{
			Freeze(_temporary);
			_temporary = null;
		}

		internal void Clear()
		{
			_keyToIndex.Clear();
			_values = null;
		}

		private void Freeze(Dictionary<SerializableGuid, TValue> source)
		{
			_values = new TValue[source.Count];
			_keyToIndex = new Dictionary<SerializableGuid, int>(source.Count);
			_idToIndex = new Dictionary<string, int>(source.Count);

			int index = 0;
			foreach (var entry in source)
			{
				_values[index] = entry.Value;

				_keyToIndex[entry.Key] = index;

				if (entry.Value is IIdentifiable identifiable)
					_idToIndex[identifiable.Id] = index;

				entry.Value.SetIndex(index);

				index++;
			}
		}

		public bool Contains(in SerializableGuid guid) => _keyToIndex.ContainsKey(guid);
		public bool Contains(string id) => _idToIndex.ContainsKey(id);
		public bool Contains(int index) => _values.ContainsIndexSafe(index);

		public bool TryAdd(in SerializableGuid guid, TValue value) => _temporary.TryAdd(guid, value);
		public bool Remove(in SerializableGuid guid) => _temporary.Remove(guid);
	}
}
