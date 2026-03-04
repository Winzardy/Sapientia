#if !UNITY_EDITOR && CLIENT
#define CONTENT_ENTRY_BUFFER
#endif
#if CONTENT_ENTRY_BUFFER
using System.Collections.Generic;

namespace Content.Management
{
	public static class ContentEntryBuffer
	{
		private static Dictionary<SerializableGuid, IUniqueContentEntry> _dictionary = new();

		internal static void Clear() => _dictionary.Clear();
		internal static void Push(IUniqueContentEntry entry) => _dictionary.Add(entry.Guid, entry);
		internal static void Remove(IUniqueContentEntry entry) => Remove(entry.Guid);
		internal static void Remove(SerializableGuid guid) => _dictionary.Remove(guid);

		internal static IUniqueContentEntry Pop(SerializableGuid guid, bool remove = true)
		{
			var entry = _dictionary[guid];
			if (remove)
				Remove(entry.Guid);
			return entry;
		}
	}
}
#endif
