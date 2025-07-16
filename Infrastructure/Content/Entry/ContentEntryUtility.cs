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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static ref readonly T Get<T>(this in SerializableGuid guid, ref int index)
		{
#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying)
#endif
				if (index >= 0 && ContentManager.Contains<T>(index))
				{
					var entryByIndex = ContentManager.GetEntry<T>(index);
					if (entryByIndex.Guid == guid)
						return ref entryByIndex.Value;
				}

			var entryByGuid = ContentManager.GetEntry<T>(in guid);
			index = entryByGuid.Index;
			return ref entryByGuid.Value;
		}
	}
}
