#if !UNITY_EDITOR && CLIENT
#define CONTENT_ENTRY_BUFFER
#endif
using System.Collections.Generic;
using Content.Management;
using Sapientia.Collections;
using Sapientia.Pooling;
using Sapientia.Reflection;

namespace Content
{
	public partial class BaseContentEntry<T>
	{
		private IUniqueContentEntry[] _registeredNestedEntries;

		public virtual IReadOnlyDictionary<SerializableGuid, MemberReflectionReference<IUniqueContentEntry>> Nested => null;

		private void OnNestedRegister()
		{
			var isNullOrEmpty = Nested.IsNullOrEmpty();

			if (isNullOrEmpty)
				return;

			using (ListPool<IUniqueContentEntry>.Get(out var list))
			{
				foreach (var (guid, reference) in Nested)
				{
#if CONTENT_ENTRY_BUFFER
					var entry = ContentEntryBuffer.Get(guid);
#else
					// Resolve происходит через рефлексию
					var entry = reference.Resolve(this, true);
#endif
					entry.SetParent(this);
					entry.Register();

					list.Add(entry);
				}

				_registeredNestedEntries = list.ToArray();
			}
		}

		private void OnNestedUnregister()
		{
			if (_registeredNestedEntries.IsNullOrEmpty())
				return;

			foreach (var entry in _registeredNestedEntries)
				entry.Unregister();
		}
	}

	public partial interface IContentEntry
	{
		IReadOnlyDictionary<SerializableGuid, MemberReflectionReference<IUniqueContentEntry>> Nested { get; }
	}
}
