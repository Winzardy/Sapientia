using System.Runtime.CompilerServices;

namespace Content
{
	public static class ContentUtility
	{
		/// <summary>
		/// Создание уникального идентификатора по guid и маркеру
		/// </summary>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <param name="mark">Маркер по которому можно определить источник</param>
		/// <returns>Уникальный идентификатор</returns>
		public static string Combine(in SerializableGuid guid, string mark) => $"{mark}_{guid}";

		/// <summary>
		/// Возвращает значение контента по <paramref name="guid"/>, пытаясь использовать кеш индекса.
		/// В отличие от <see cref="ContentManager.Get{T}(in SerializableGuid)"/>, этот метод принимает/обновляет
		/// <paramref name="index"/> для O(1) доступа при повторных вызовах
		/// </summary>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <param name="index">Индексация</param>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns>Контент типа <typeparamref name="T"/></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T GetContentValue<T>(this in SerializableGuid guid, ref int index)
		{
			if (guid == ContentReference.Single.Guid)
				return ref ContentManager.Get<T>();

#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying)
#endif
				if (index >= 0
				    && ContentManager.TryGetEntry<T>(index, out var entryByIndex)
				    && entryByIndex == guid)
				{
					return ref entryByIndex.Value;
				}

			var entryByGuid = ContentManager.GetEntry<T>(in guid);
			index = entryByGuid.Index;
			return ref entryByGuid.Value;
		}

		/// <summary>
		/// Возвращает значение контента по <paramref name="guid"/>, пытаясь использовать кеш индекса.
		/// В отличие от <see cref="GetContentValue{T}"/>, этот метод не восстанавливает
		/// <paramref name="index"/>
		/// </summary>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <param name="index">Индексация</param>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns>Контент типа <typeparamref name="T"/></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T ReadContentValue<T>(this in SerializableGuid guid, int index = -1)
		{
			if (guid == ContentReference.Single.Guid)
				return ref ContentManager.Get<T>();

#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying)
#endif
				if (index >= 0
				    && ContentManager.TryGetEntry<T>(index, out var entryByIndex)
				    && entryByIndex == guid)
				{
					return ref entryByIndex.Value;
				}

			var entryByGuid = ContentManager.GetEntry<T>(in guid);
			return ref entryByGuid.Value;
		}
	}
}
