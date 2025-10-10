using System;
using System.Runtime.CompilerServices;
using Sapientia;

namespace Content
{
	public static class ContentEntryUtility
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deconstruct<T>(this IContentEntry<T> entry,
#if CLIENT
			[JetBrains.Annotations.CanBeNull]
#endif
			out string id,
			out T value)
		{
			id = entry is IIdentifiable identifiable ? identifiable.Id : null;
			value = entry.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Deconstruct<T>(this IUniqueContentEntry<T> entry, out SerializableGuid guid, out T value)
		{
			guid = entry.Guid;
			value = entry.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEmpty<T>(this BaseContentEntry<T> entry) => entry == null || !entry.IsValid() || entry.Value == null;
	}
}
