using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Reflection;

namespace Content
{
	public partial class BaseContentEntry<T>
	{
		public virtual IReadOnlyDictionary<SerializableGuid, MemberReflectionReference<IUniqueContentEntry>> Nested => null;

		private void OnNestedRegister()
		{
			var isNullOrEmpty = Nested.IsNullOrEmpty();

			if (isNullOrEmpty)
				return;

			// Resolve происходит через рефлексию, самый быстрый вариант это code-gen (roslyn), есть идеи, но пока так
			foreach (var reference in Nested.Values)
			{
				var entry = reference.Resolve(this, true);
				entry.SetParent(this);
				entry.Register();
			}
		}

		private void OnNestedUnregister()
		{
			if (Nested.IsNullOrEmpty())
				return;

			// Resolve происходит через рефлексию, самый быстрый вариант это code-gen (roslyn), есть идеи, но пока так
			// Проблемы кодоген:
			// - Доступ к вложенным приватным ContentEntry, была идея решать это через дерево NestedEntries, но вариант:
			//   (field -> privateField1 -> privateField2 -> contentEntry) она не решит.
			// - Долго и муторно делать
			foreach (var (key, reference) in Nested)
			{
				var entry = reference.Resolve(this);

				if (ContentDebug.Logging.Nested.resolve)
				{
					if (entry == null)
						ContentDebug.LogError($"Nested entry by guid [ {key} ] is null! by path [ {reference.Path} ]", Context);
				}

				//IL2CPP может "strip" не нужное поле! поэтому проверка на Null
				entry?.Unregister();
			}
		}
	}

	public partial interface IContentEntry
	{
		public IReadOnlyDictionary<SerializableGuid, MemberReflectionReference<IUniqueContentEntry>> Nested { get; }
	}
}
