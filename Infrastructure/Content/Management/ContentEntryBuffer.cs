#if !UNITY_EDITOR && CLIENT
#define CONTENT_ENTRY_BUFFER
#endif
#if CONTENT_ENTRY_BUFFER
using System.Collections.Generic;

namespace Content
{
	public static class ContentEntryBuffer
	{
		private static Dictionary<SerializableGuid, IUniqueContentEntry> _dictionary = new();

		internal static void Clear() => _dictionary.Clear();

		internal static void Add(IUniqueContentEntry entry)
		{
#if UNITY_EDITOR
			if(_dictionary.ContainsKey(entry.Guid))
				return;
#endif
			_dictionary.Add(entry.Guid, entry);
		}

		internal static void Remove(IUniqueContentEntry entry) => Remove(entry.Guid);
		internal static void Remove(SerializableGuid guid) => _dictionary.Remove(guid);

		internal static IUniqueContentEntry Get(SerializableGuid guid) => _dictionary[guid];

		internal static IUniqueContentEntry Take(SerializableGuid guid)
		{
			var entry = Get(guid);
			Remove(entry.Guid);
			return entry;
		}
	}
}
#endif
