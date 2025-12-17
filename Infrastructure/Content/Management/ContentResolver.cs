#if DebugLog
#define ENABLE_CONTENT_CONTAINS_CHECK
#endif

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Reflection;
#if ENABLE_CONTENT_CONTAINS_CHECK
using Sapientia.Extensions;
#endif

namespace Content.Management
{
	/// <summary>
	/// Предоставляет доступ к контент-записям по типу, идентификатору или GUID.
	/// Получает контент от конфигуратора, активирует его и предоставляет API для выборки и проверки наличия.
	/// </summary>
	/// <remarks>
	/// ⚠️ Важно: нет поддержки полиморфизма. Хранилище организовано строго по типу.
	/// Элементы дочерних типов не будут возвращены при запросе по базовому типу.
	/// </remarks>
	public sealed partial class ContentResolver : IDisposable
	{
		private List<IContentEntry> _entries = new();


		public void Dispose()
		{
			Clear();
		}

		internal async Task PopulateAsync(IContentImporter importer, CancellationToken token = default)
		{
			var entries = await importer.ImportAsync(token);

			if (token.IsCancellationRequested)
				return;

			_entries.AddRange(entries);

			foreach (var entry in entries)
				entry.Register();

			ContentEntryMap.Populated?.Invoke(entries);

			// Очищаем кеш, чтобы не забивать память лишними FieldInfo
			FastReflection.Clear();
		}

		private void Clear()
		{
			foreach (var entry in _entries)
				entry.Unregister();

			_entries = null;
			ContentEntryMap.Cleared?.Invoke();
		}

		/// <summary>
		/// Проверяет наличие любой записи контента типа <typeparamref name="T"/> —
		/// как одиночной, так и уникальной
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns><c>true</c>, если запись типа <typeparamref name="T"/> существует; иначе <c>false</c></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool Any<T>()
		{
			if (SingleContentEntryShortcut<T>.Contains())
				return true;

			return ContentEntryMap<T>.Any();
		}

		/// <summary>
		/// Проверяет наличие уникального контента типа <typeparamref name="T"/> по <paramref name="guid"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <returns><c>true</c>, если запись найдена; иначе <c>false</c></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool Contains<T>(in SerializableGuid guid)
			=> ContentEntryMap<T>.Contains(in guid);

		/// <summary>
		/// Проверяет наличие уникального контента типа <typeparamref name="T"/> по строковому (<see cref="string"/>) <paramref name="id"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="id">Строковый идентификатор записи</param>
		/// <returns><c>true</c>, если запись найдена; иначе <c>false</c></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool Contains<T>(string id) => ContentEntryMap<T>.Contains(id);

		/// <summary>
		/// Проверяет наличие уникального контента типа <typeparamref name="T"/> по индексу (<see cref="int"/>) <paramref name="index"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="index">Строковый идентификатор записи</param>
		/// <returns><c>true</c>, если запись найдена; иначе <c>false</c></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool Contains<T>(int index) => ContentEntryMap<T>.Contains(index);

		/// <summary>
		/// Проверяет наличие одиночного контента типа <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns><c>true</c>, если запись найдена; иначе <c>false</c></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool Contains<T>() => SingleContentEntryShortcut<T>.Contains();

		/// <summary>
		/// Получает <b>уникальную запись</b> контента типа <typeparamref name="T"/> по <paramref name="guid"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal UniqueContentEntry<T> GetEntry<T>(in SerializableGuid guid)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (guid == Guid.Empty)
				throw new Exception($"Empty guid for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(guid))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with guid: [ {guid} ]");
#endif
			return ContentEntryMap<T>.GetEntry(in guid);
		}

		/// <summary>
		/// Получает <b>уникальную запись</b> контента типа <typeparamref name="T"/> по строковому (<see cref="string"/>) <paramref name="id"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="id">Строковый идентификатор записи</param>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal UniqueContentEntry<T> GetEntry<T>(string id)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (id.IsNullOrEmpty())
				throw new Exception($"Empty id for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(id))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with id: [ {id} ]");
#endif
			return ContentEntryMap<T>.GetEntry(id);
		}

		/// <summary>
		/// Получает <b>уникальную запись</b> контента типа <typeparamref name="T"/> по индексу (<see cref="int"/>) <paramref name="index"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="index">Строковый идентификатор записи</param>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal UniqueContentEntry<T> GetEntry<T>(int index)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK

			if (index == ContentConstants.INVALID_INDEX)
				throw new Exception($"Invalid index for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(index))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with index: [ {index} ]");
#endif
			return ContentEntryMap<T>.GetEntry(index);
		}

		/// <summary>
		/// Получает одиночную запись контента типа <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal SingleContentEntry<T> GetEntry<T>()
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (!Contains<T>())
				throw new Exception($"Could not find single entry of type: [ {typeof(T).Name} ]");
#endif
			return SingleContentEntryShortcut<T>.GetEntry();
		}

		/// <summary>
		/// Получает <b>уникальную запись</b> контента типа <typeparamref name="T"/> по <paramref name="guid"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <param name="entry">Запись</param>
		/// <returns>Успешность нахождения записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool TryGetEntry<T>(in SerializableGuid guid, out UniqueContentEntry<T> entry)
			=> ContentEntryMap<T>.TryGetEntry(in guid, out entry);

		/// <summary>
		/// Получает <b>уникальную запись</b> контента типа <typeparamref name="T"/> по строковому (<see cref="string"/>) <paramref name="id"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="id">Строковый идентификатор записи</param>
		/// <param name="entry">Запись</param>
		/// <returns>Успешность нахождения записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool TryGetEntry<T>(string id, out UniqueContentEntry<T> entry)
			=> ContentEntryMap<T>.TryGetEntry(id, out entry);

		/// <summary>
		/// Получает <b>уникальную запись</b> контента типа <typeparamref name="T"/> по индексу (<see cref="int"/>) <paramref name="index"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="index">Строковый идентификатор записи</param>
		/// <param name="entry">Запись</param>
		/// <returns>Успешность нахождения записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool TryGetEntry<T>(int index, out UniqueContentEntry<T> entry)
			=> ContentEntryMap<T>.TryGetEntry(index, out entry);

		/// <summary>
		/// Получает одиночную запись контента типа <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns>Успешность нахождения записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal bool TryGetEntry<T>(out SingleContentEntry<T> entry)
			=> SingleContentEntryShortcut<T>.TryGetEntry(out entry);

		/// <summary>
		/// Получает уникальную запись контента типа <typeparamref name="T"/> по <paramref name="guid"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref readonly T Get<T>(in SerializableGuid guid)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (guid == Guid.Empty)
				throw new Exception($"Empty guid for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(guid))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with guid: [ {guid} ]");
#endif
			return ref ContentEntryMap<T>.Get(in guid);
		}

		/// <summary>
		/// Получает уникальный контент типа <typeparamref name="T"/> по строковому (<see cref="string"/>) <paramref name="id"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="id">Строковый идентификатор записи</param>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref readonly T Get<T>(string id)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (id.IsNullOrEmpty())
				throw new Exception($"Empty id for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(id))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with id: [ {id} ]");
#endif
			return ref ContentEntryMap<T>.Get(id);
		}

		/// <summary>
		/// Получает уникальный контент типа <typeparamref name="T"/> по индексу (<see cref="int"/>) <paramref name="index"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="index">Строковый идентификатор записи</param>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref readonly T Get<T>(int index)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK

			if (index == ContentConstants.INVALID_INDEX)
				throw new Exception($"Invalid index for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(index))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with index: [ {index} ]");
#endif
			return ref ContentEntryMap<T>.Get(index);
		}

		/// <summary>
		/// Получает контента типа <typeparamref name="T"/> из одиночной записи
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns>Запись (только для чтения) на найденный контент типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref readonly T Get<T>()
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (!Contains<T>())
				throw new Exception($"Could not find single entry of type: [ {typeof(T).Name} ]");
#endif
			return ref SingleContentEntryShortcut<T>.GetEntry().Value;
		}

		/// <summary>
		/// Возвращает все записи контента типа <typeparamref name="T"/> (как одиночные, так и уникальные)
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns>Перечисление всех записей типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal IEnumerable<IContentEntry<T>> GetAllEntries<T>()
		{
			if (Contains<T>())
				yield return SingleContentEntryShortcut<T>.GetEntry();

			if (!ContentEntryMap<T>.Any())
				yield break;

			foreach (var entry in ContentEntryMap<T>.GetAll())
				yield return entry;
		}

		/// <summary>
		/// Возвращает все значения контента типа <typeparamref name="T"/> (как одиночные, так и уникальные)
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <returns>Перечисление всех значений типа <typeparamref name="T"/></returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal IEnumerable<ContentReference<T>> GetAll<T>()
		{
			foreach (var entry in GetAllEntries<T>())
				yield return entry.ToReference();
		}

		/// <summary>
		/// Получает строковый (<see cref="string"/>) идентификатор записи по заданному <paramref name="guid"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <returns>Строковый идентификатор записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal string ToId<T>(in SerializableGuid guid)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (guid == Guid.Empty)
				throw new Exception($"Empty guid for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(guid))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with guid: [ {guid} ]");
#endif
			return ContentEntryMap<T>.ToId(in guid);
		}

		/// <summary>
		/// Получает строковый (<see cref="string"/>) идентификатор записи по индексу (<see cref="int"/>)
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="index">Индекс записи</param>
		/// <returns>Строковый идентификатор записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal string ToId<T>(int index)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (index == ContentConstants.INVALID_INDEX)
				throw new Exception($"Invalid index for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(index))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with index: [ {index} ]");
#endif
			return ContentEntryMap<T>.ToId(index);
		}

		/// <summary>
		/// Получает <see cref="SerializableGuid"/> по строковому <paramref name="id"/> для записи типа <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="id">Строковый идентификатор записи</param>
		/// <returns>Уникальный идентификатор записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref readonly SerializableGuid ToGuid<T>(string id)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (id.IsNullOrEmpty())
				throw new Exception($"Empty id for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(id))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with id: [ {id} ]");
#endif
			return ref ContentEntryMap<T>.ToGuid(id);
		}

		/// <summary>
		/// Получает индекс (<see cref="int"/>) записи по заданному <paramref name="guid"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <returns>Индекс записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal int ToIndex<T>(in SerializableGuid guid)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (guid == Guid.Empty)
				throw new Exception($"Empty guid for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(guid))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with guid: [ {guid} ]");
#endif
			return ContentEntryMap<T>.ToIndex(in guid);
		}

		/// <summary>
		/// Получает индекс (<see cref="int"/>) по строковому <paramref name="id"/> для записи типа <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="id">Строковый идентификатор записи</param>
		/// <returns>Индекс записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal int ToIndex<T>(string id)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (id.IsNullOrEmpty())
				throw new Exception($"Empty id for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(id))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with id: [ {id} ]");
#endif
			return ContentEntryMap<T>.ToIndex(id);
		}

		/// <summary>
		/// Получает <see cref="SerializableGuid"/> по индексу (<see cref="int"/>) <paramref name="index"/> для записи типа <typeparamref name="T"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="index">Индекс записи</param>
		/// <returns>Уникальный идентификатор записи</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal ref readonly SerializableGuid ToGuid<T>(int index)
		{
#if ENABLE_CONTENT_CONTAINS_CHECK
			if (index == ContentConstants.INVALID_INDEX)
				throw new Exception($"Invalid index for get entry of type: [ {typeof(T).Name} ]");
			if (!Contains<T>(index))
				throw new Exception($"Could not find entry of type: [ {typeof(T).Name} ] with index: [ {index} ]");
#endif
			return ref ContentEntryMap<T>.ToGuid(index);
		}

		/// <summary>
		/// Получает имя по заданному <paramref name="guid"/>
		/// </summary>
		/// <typeparam name="T">Тип контента</typeparam>
		/// <param name="guid">Уникальный идентификатор записи</param>
		/// <param name="verbose">Добавляет дополнительную информацию (id, type, guid)</param>
		/// <returns>Имя по записи, если она найдена; иначе — <c>guid</c> или пустая строка (если <b>guid</b> пустой)</returns>
		/// <remarks>
		/// ⚠️ Важно: нет поддержки полиморфизма
		/// </remarks>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal string ToLabel<T>(in SerializableGuid guid, bool verbose = false)
		{
			if (guid == IContentReference.SINGLE_GUID)
			{
				if (Contains<T>())
				{
					var entry = GetEntry<T>();
					return verbose
						? $"{ContentConstants.DEFAULT_SINGLE_ID} (type: {entry.ValueType.Name})"
						: $"{ContentConstants.DEFAULT_SINGLE_ID}";
				}
			}
			else if (Contains<T>(in guid))
			{
				var entry = GetEntry<T>(in guid);
				return verbose
					? $"{entry.Id} (type:{entry.ValueType.Name}, guid: {guid})"
					: $"{entry.Id}";
			}

			return verbose
				? $"[{typeof(T).Name}] {guid}"
				: guid.ToString();
		}
	}
}
