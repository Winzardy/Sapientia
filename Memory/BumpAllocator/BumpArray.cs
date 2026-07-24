using System;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.Memory
{
	/// <summary>
	/// Массив фиксированной длины в bump-арене. Хранит только self-relative смещение от адреса своего
	/// поля <see cref="offset"/> до данных и длину — никаких ссылок на <see cref="BumpHeader"/> не нужно,
	/// данные находятся той же арене и адресуются как <c>&amp;offset + offset</c>. Position-independent,
	/// переживает перемещение/serialize вместе с ареной. Память поэлементно не освобождает.
	///
	/// <see cref="Alloc"/> вызывается на уже размещённой в арене структуре (через <c>ref</c>), а не на
	/// временной: self-relative смещение считается от финального адреса поля.
	/// </summary>
	public unsafe struct BumpArray<T> where T : unmanaged
	{
		/// <summary>Self-relative смещение от адреса этого поля до данных в арене.</summary>
		public RelativePtr<T> offset;
		public int length;

		public readonly bool IsValid => length > 0;
		public readonly int Length => length;

		/// <summary>
		/// Выделяет данные на <paramref name="length"/> элементов и записывает self-relative
		/// <see cref="offset"/> до них. <paramref name="length"/> &lt;= 0 — пусто, без аллокации.
		/// </summary>
		public void Alloc(ref BumpHeader allocator, int length)
		{
			if (length <= 0)
			{
				offset = default;
				this.length = 0;
				return;
			}

			var dataPtr = allocator.MemAlloc<T>(length, out _);
			// offset = адрес данных - адрес поля offset (оба в арене) -> self-relative.
			offset.SetPtr(dataPtr);
			this.length = length;
		}

		public void Alloc(ref BumpHeader allocator, Span<T> values)
		{
			Alloc(ref allocator, values.Length);
			values.CopyTo(GetSpan());
		}

		public SafePtr<T> GetPtr()
		{
			var fieldPtr = (SafePtr)offset.AsSafePtr();
			return new SafePtr(fieldPtr.ptr + offset.byteOffset, length * TSize<T>.size).Cast<T>();
		}

		public ref T Get(int index)
		{
			E.ASSERT((uint)index < (uint)length, "[BumpArray] Индекс за пределами массива.");
			return ref GetPtr()[index];
		}

		public Span<T> GetSpan()
		{
			return GetPtr().GetSpan(length);
		}
	}
}
