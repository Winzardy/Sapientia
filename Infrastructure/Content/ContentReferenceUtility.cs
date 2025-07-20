using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Pooling;

namespace Content
{
	public static class ContentReferenceUtility
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IContentEntry<T> GetEntry<T>(this in ContentReference<T> reference)
			=> reference.IsSingle ? ContentManager.GetEntry<T>() : ContentManager.GetEntry<T>(in reference.guid);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(this ref ContentReference<T> reference)
		{
			if (reference.IsSingle)
				return ref ContentManager.Get<T>();

#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying)
#endif
				if (reference.index >= 0
				    && ContentManager.TryGetEntry<T>(reference.index, out var entryByIndex)
				    && entryByIndex == reference.guid)
				{
					return ref entryByIndex.Value;
				}

			var entryByGuid = ContentManager.GetEntry<T>(in reference.guid);
			reference.index = entryByGuid.Index;
			return ref entryByGuid.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetId<T>(this in ContentReference<T> reference) =>
			ContentManager.ToId<T>(in reference.guid);

		/// <summary>
		/// Возвращает ссылку на содержимое по ссылке <paramref name="reference"/>.
		/// Если ссылка пустая, будет использован <paramref name="defaultEntryId"/> для загрузки контента по умолчанию.
		/// </summary>
		/// <param name="defaultEntryId">Идентификатор записи по умолчанию, используемый если <paramref name="reference"/> пустой.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this in ContentReference<T> reference, string defaultEntryId)
		{
			if (reference.IsEmpty())
				return ref ContentManager.Get<T>(defaultEntryId);

			return ref Read(reference);
		}

		/// <summary>
		/// Разница между <see cref="Get{T}(ref Content.ContentReference)"/>, тем что он не восстанавливает Index
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this in ContentReference<T> reference)
		{
			if (reference.IsSingle)
				return ref ContentManager.Get<T>();

#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying)
#endif
				if (reference.index >= 0 && ContentManager.Contains<T>(reference.index))
				{
					var entryByIndex = ContentManager.GetEntry<T>(reference.index);
					if (entryByIndex.Guid == reference.guid)
						return ref entryByIndex.Value;
				}

			var entryByGuid = ContentManager.GetEntry<T>(in reference.guid);
			return ref entryByGuid.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToId<T>(this in ContentReference<T> reference)
			=> GetId(in reference);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IContentEntry<T> GetEntry<T>(this in ContentReference reference)
			=> reference.IsSingle ? ContentManager.GetEntry<T>() : ContentManager.GetEntry<T>(in reference.guid);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Get<T>(this ref ContentReference reference)
		{
			if (reference.IsSingle)
				return ref ContentManager.Get<T>();

			if (reference.index >= 0 && ContentManager.Contains<T>(reference.index))
			{
				var entryByIndex = ContentManager.GetEntry<T>(reference.index);
				if (entryByIndex.Guid == reference.guid)
					return ref entryByIndex.Value;
			}

			var entryByGuid = ContentManager.GetEntry<T>(in reference.guid);
			reference.index = entryByGuid.Index;
			return ref entryByGuid.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string GetId<T>(this in ContentReference reference) =>
			ContentManager.ToId<T>(in reference.guid);

		/// <summary>
		/// Разница между <see cref="Get{T}(ref Content.ContentReference)"/>, тем что он не восстанавливает Index
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this in ContentReference reference)
		{
			if (reference.IsSingle)
				return ref ContentManager.Get<T>();

			if (reference.index >= 0 && ContentManager.Contains<T>(reference.index))
			{
				var entryByIndex = ContentManager.GetEntry<T>(reference.index);
				if (entryByIndex.Guid == reference.guid)
					return ref entryByIndex.Value;
			}

			var entryByGuid = ContentManager.GetEntry<T>(in reference.guid);
			return ref entryByGuid.Value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToId<T>(this in ContentReference reference)
			=> GetId<T>(in reference);

		/// <param name="str">Либо Id, либо Guid</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ContentReference<T> ToReference<T>(string str) => new()
		{
			guid = SerializableGuid.TryParse(str, out var guid) ? guid : ContentManager.ToGuid<T>(str)
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ContentReference<T> ToReference<T>(this IContentEntry<T> entry)
		{
			if (entry is IUniqueContentEntry<T> unique)
				return new(in unique.Guid, unique.Index);

			return IContentReference.SINGLE_GUID;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ContentReference<T>[] ToReferences<T>(this IList<IContentEntry<T>> list)
		{
			using (ListPool<ContentReference<T>>.Get(out var result))
			{
				for (int i = 0; i < list.Count; i++)
					result.Add(list[i].ToReference());

				return result.ToArray();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToLabel<T>(this in ContentReference<T> reference, bool verbose = false) =>
			ContentManager.ToLabel<T>(in reference.guid, verbose);
	}
}
