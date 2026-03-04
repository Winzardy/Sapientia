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
		public virtual IReadOnlyDictionary<SerializableGuid, MemberReflectionReference<IUniqueContentEntry>> Nested => null;

		private List<IUniqueContentEntry> _registeredNestedEntries;

		private void OnNestedRegister()
		{
			var isNullOrEmpty = Nested.IsNullOrEmpty();

			if (isNullOrEmpty)
				return;

			_registeredNestedEntries = ListPool<IUniqueContentEntry>.Get();
			foreach (var (guid, reference) in Nested)
			{
#if CONTENT_ENTRY_BUFFER
				var entry = ContentEntryBuffer.Pop(guid, false);
#else
				// Resolve происходит через рефлексию
				var entry = reference.Resolve(this, true);
#endif
				entry.SetParent(this);
				entry.Register();

				_registeredNestedEntries.Add(entry);
			}
		}

		private void OnNestedUnregister()
		{
			if (_registeredNestedEntries.IsNullOrEmpty())
				return;

			foreach (var entry in _registeredNestedEntries)
				entry.Unregister();

			StaticObjectPoolUtility.ReleaseAndSetNull(ref _registeredNestedEntries);
		}
	}

	public partial interface IContentEntry
	{
		IReadOnlyDictionary<SerializableGuid, MemberReflectionReference<IUniqueContentEntry>> Nested { get; }
	}
}
