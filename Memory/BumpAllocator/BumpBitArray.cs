using Sapientia.Extensions;

namespace Sapientia.Memory
{
	/// <summary>
	/// Битовый массив фиксированной длины в bump-арене: обёртка над <see cref="BumpArray{T}"/> из
	/// 64-битных слов. Как и <see cref="BumpArray{T}"/> — self-relative и position-independent,
	/// переживает перемещение/serialize вместе с ареной; проверка/установка бита — O(1).
	///
	/// <see cref="Alloc"/> вызывается на уже размещённой в арене структуре (через <c>ref</c>), а не на
	/// временной (см. <see cref="BumpArray{T}.Alloc"/>); слова зануляются явно — содержимое (включая
	/// хвостовые биты последнего слова) детерминировано независимо от политики зануления аллокатора.
	/// </summary>
	public struct BumpBitArray
	{
		private const int WORD_SHIFT = 6;
		private const int BIT_MASK = 63;

		/// <summary>Публичен ради валидации диапазона данных по ref (как <see cref="BumpString.encodedString"/>).</summary>
		public BumpArray<ulong> words;
		/// <summary>Длина в битах.</summary>
		public int length;

		public readonly bool IsValid => length > 0;
		public readonly int Length => length;

		/// <summary>Число слов под <paramref name="length"/> бит.</summary>
		public static int GetWordsCount(int length)
		{
			return (length + BIT_MASK) >> WORD_SHIFT;
		}

		/// <summary>Размер данных в арене для предрасчёта резервируемого размера (сама структура не входит).</summary>
		public static int GetReservedSize(int length)
		{
			return GetWordsCount(length) * TSize<ulong>.size;
		}

		/// <summary>
		/// Выделяет слова под <paramref name="length"/> бит (все сброшены) и запоминает длину.
		/// <paramref name="length"/> &lt;= 0 — пусто, без аллокации.
		/// </summary>
		public void Alloc(ref BumpHeader allocator, int length)
		{
			if (length <= 0)
			{
				words = default;
				this.length = 0;
				return;
			}

			words.Alloc(ref allocator, GetWordsCount(length));
			words.GetSpan().Clear();
			this.length = length;
		}

		public bool IsSet(int index)
		{
			E.ASSERT(index >= 0 && index < length, "[BumpBitArray] Индекс за пределами массива.");
			return (words.Get(index >> WORD_SHIFT) & (1ul << (index & BIT_MASK))) != 0;
		}

		public void Set(int index, bool value)
		{
			E.ASSERT(index >= 0 && index < length, "[BumpBitArray] Индекс за пределами массива.");
			ref var word = ref words.Get(index >> WORD_SHIFT);
			var bit = 1ul << (index & BIT_MASK);
			if (value)
				word |= bit;
			else
				word &= ~bit;
		}
	}
}
