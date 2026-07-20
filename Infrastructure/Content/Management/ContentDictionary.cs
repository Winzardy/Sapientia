using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;

namespace Content.Management
{
	public sealed class ContentDictionary<TEntry>
		where TEntry : IUniqueContentEntry
	{
		private List<TEntry> _temporary;

		// Можно будет переделать на FrozenDictionary, когда его завезут в Unity
		private Dictionary<SerializableGuid, int> _keyToIndex;

		// Можно будет переделать на FrozenDictionary, когда его завезут в Unity
		private Dictionary<string, int> _idToIndex;

		// Постепенно прогревается при запросе Id для объекта!
		private Dictionary<SerializableGuid, string> _keyToId;

		private TEntry[] _entries;

		public ref readonly TEntry this[in SerializableGuid key]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _entries[_keyToIndex[key]];
		}

		public ref readonly TEntry this[string id]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _entries[_idToIndex[id]];
		}

		public ref readonly TEntry this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref _entries[index];
		}

		public int Count => _entries.Length;
		public int Length => _entries.Length;

		public TEntry[] Entries => _entries;

		public bool IsEmpty => _entries == null || _entries.Length == 0;

		public bool IsFrozen => _entries != null;
		public bool IsBuilding => _temporary != null;

		public ContentDictionary()
		{
		}

		public ContentDictionary(ICollection<TEntry> source)
		{
			Fill(source);
		}

		internal void Unfreeze()
		{
			if (_temporary != null)
				throw ContentDebug.Exception("Already building...");

			_temporary = new();
		}

		internal void Freeze()
		{
			if (_temporary == null)
				throw ContentDebug.Exception("Already frozen...");

			Fill(_temporary);
			_temporary = null;
		}

		internal void Clear()
		{
			_idToIndex.Clear();
			_idToIndex = null;
			_keyToIndex.Clear();
			_keyToIndex = null;
			_entries = null;
		}

		private void Fill(ICollection<TEntry> source)
		{
			if (_entries != null)
			{
				var array = new TEntry[_entries.Length + source.Count];
				foreach (var (entry, i) in _entries.WithIndex())
					array[i] = entry;

				_entries = array;
			}
			else
			{
				_entries = new TEntry[source.Count];
				_keyToIndex = new Dictionary<SerializableGuid, int>(source.Count);
				_idToIndex = new Dictionary<string, int>(source.Count);
				_keyToId = new Dictionary<SerializableGuid, string>();
			}

			int index = 0;
			foreach (var entry in source)
			{
				_entries[index] = entry;

				_keyToIndex[entry.Guid] = index;

				if (!entry.Id.IsNullOrEmpty())
				{
					_keyToId[entry.Guid] = entry.Id;
					_idToIndex[entry.Id] = index;
				}

				entry.SetIndex(index);
				index++;
			}
		}

		public bool Contains(in SerializableGuid guid) => _keyToIndex.ContainsKey(guid);
		public bool Contains(string id) => _idToIndex.ContainsKey(id);
		public bool Contains(int index) => _entries.ContainsIndexSafe(index);

		public void Stage(TEntry entry) => _temporary.Add(entry);
		public bool Unstage(TEntry entry) => _temporary.Remove(entry);

		internal bool TryGet(in SerializableGuid guid, out TEntry entry)
		{
			if (IsFrozen && _keyToIndex.TryGetValue(guid, out var index))
			{
				entry = _entries[index];
				return true;
			}

			var yGuid = guid;
			return _temporary.TryGetFirst(x => x.Guid == yGuid, out entry);
		}

		internal string GetId(in SerializableGuid guid)
		{
			if (!_keyToId.TryGetValue(guid, out var id))
				_keyToId[guid] = id = guid.ToString();
			return id;
		}

		internal string GetId(int index)
		{
			ref readonly var guid = ref _entries[index].Guid;
			if (!_keyToId.TryGetValue(guid, out var id))
				_keyToId[guid] = id = guid.ToString();
			return id;
		}
	}
}
