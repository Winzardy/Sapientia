using Sapientia;

namespace Content.Management
{
	public static class SingleContentEntryShortcut<T>
	{
		private static SingleContentEntry<T> _entry;

		internal static void Register(SingleContentEntry<T> entry)
		{
			if (Contains())
				throw ContentDebug.Exception($"Already registered single entry of type: [ {typeof(T).Name} ]");

			_entry = entry;

			if (entry.Value is IExternallyIdentifiable identifiable)
				identifiable.SetId(ContentConstants.DEFAULT_SINGLE_ID);
		}

		internal static void Unregister() => _entry = null;

		internal static ref readonly T Get() => ref _entry.Value;
		internal static SingleContentEntry<T> GetEntry() => _entry;

		internal static bool Contains() => _entry != null;
	}
}
