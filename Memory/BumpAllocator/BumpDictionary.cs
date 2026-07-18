using System;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace Sapientia.Memory
{
	/// <summary>
	/// Read-only словарь в bump-арене. Строится один раз из обычного <see cref="Dictionary{TKey,TValue}"/>
	/// через <see cref="Fill"/> и после этого только читается — добавление/удаление не поддерживается.
	/// Как и <see cref="BumpArray{T}"/>, хранит только self-relative смещения: position-independent,
	/// переживает перемещение/serialize арены без фиксапов.
	///
	/// Layout классический: buckets → индекс головы цепочки в entries, коллизии через <see cref="Entry.next"/>.
	/// Порядок entries повторяет порядок перечисления источника — для детерминированного бинаря заполняйте
	/// исходный словарь в детерминированном порядке (Dictionary без удалений перечисляется в порядке вставки).
	///
	/// <see cref="Fill"/> вызывается на уже размещённой в арене структуре (через <c>ref</c>), а не на
	/// временной — self-relative смещения считаются от финальных адресов полей (см. <see cref="BumpArray{T}.Alloc"/>).
	/// </summary>
	public struct BumpDictionary<TKey, TValue>
		where TKey : unmanaged, IEquatable<TKey>
		where TValue : unmanaged
	{
		public struct Entry
		{
			public TKey key;
			public TValue value;
			/// <summary>Индекс следующей entry в цепочке бакета; -1 — конец цепочки.</summary>
			public int next;
		}

		public BumpArray<int> buckets;
		/// <summary>
		/// Пары в порядке вставки источника. Публичны для детерминированного перечисления содержимого.
		/// </summary>
		public BumpArray<Entry> entries;

		public readonly int Count => entries.Length;
		public readonly bool IsCreated => buckets.IsValid;

		/// <summary>
		/// Размер данных словаря в арене (buckets + entries) для предрасчёта резервируемого размера.
		/// Сама структура (поля <see cref="buckets"/>/<see cref="entries"/>) в размер не входит —
		/// она лежит в родительской структуре.
		/// </summary>
		public static int GetReservedSize(int count)
		{
			return GetBucketsCount(count) * TSize<int>.size + count * TSize<Entry>.size;
		}

		/// <summary>
		/// Число бакетов: ближайшая сверху степень двойки от count (минимум 1) — маска вместо деления по модулю.
		/// </summary>
		public static int GetBucketsCount(int count)
		{
			var result = 1;
			while (result < count)
				result <<= 1;
			return result;
		}

		/// <summary>
		/// Заполняет словарь данными <paramref name="source"/>. Аллоцирует ровно
		/// <see cref="GetReservedSize"/> байт из <paramref name="allocator"/>.
		/// </summary>
		public void Fill(ref BumpHeader allocator, Dictionary<TKey, TValue> source)
		{
			var count = source.Count;
			var bucketsCount = GetBucketsCount(count);
			buckets.Alloc(ref allocator, bucketsCount);
			entries.Alloc(ref allocator, count);

			var bucketsSpan = buckets.GetSpan();
			bucketsSpan.Fill(-1);
			if (count == 0)
			{
				return;
			}

			var index = 0;
			var entriesSpan = entries.GetSpan();
			foreach (var pair in source)
			{
				ref var entry = ref entriesSpan[index];
				entry.key = pair.Key;
				entry.value = pair.Value;

				var bucket = pair.Key.GetHashCode() & (bucketsCount - 1);
				entry.next = bucketsSpan[bucket];
				bucketsSpan[bucket] = index;
				index++;
			}
		}

		/// <summary>
		/// Читает значение КОПИЕЙ. Если TValue содержит self-relative данные (<see cref="BumpArray{T}"/> и т.п.),
		/// копия невалидна - используйте <see cref="TryGetIndex"/> и доступ к <see cref="entries"/> по ref.
		/// </summary>
		public bool TryGetValue(in TKey key, out TValue value)
		{
			if (!TryGetIndex(in key, out var index))
			{
				value = default;
				return false;
			}

			value = entries.Get(index).value;
			return true;
		}

		/// <summary>
		/// Индекс entry ключа в <see cref="entries"/> - стабильный дескриптор, который можно хранить
		/// вне арены и разыменовывать по ref без копирования значения.
		/// </summary>
		public bool TryGetIndex(in TKey key, out int index)
		{
			if (entries.Length == 0)
			{
				index = -1;
				return false;
			}

			index = buckets.Get(key.GetHashCode() & (buckets.Length - 1));
			while (index >= 0)
			{
				ref var entry = ref entries.Get(index);
				if (entry.key.Equals(key))
				{
					return true;
				}
				index = entry.next;
			}

			return false;
		}

		public bool ContainsKey(in TKey key)
		{
			return TryGetValue(in key, out _);
		}
	}
}
