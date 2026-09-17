using System;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;
using Submodules.Sapientia.Memory;

namespace Sapientia.Memory
{
	/// <summary>
	/// Bump-аллокатор фиксированного размера: только выделяет память (сдвигает _rover), но не освобождает
	/// отдельные объекты — весь блок живёт и освобождается целиком.
	///
	/// ВАЖНО — это <b>inline-заголовок, лежащий в байте 0 своего же блока</b>, а не значение-хэндл.
	/// База блока выводится из адреса самой структуры (<see cref="Memory"/> == <c>&amp;this</c>), поэтому
	/// внутри не хранится ни одного абсолютного указателя — структура position-independent и переживает
	/// перемещение/serialize/deserialize без re-resolve.
	///
	/// <see cref="BumpHeader"/> сам <b>не владеет памятью</b>: блок выделяет и освобождает обёртка
	/// (<c>RawBumpAllocator</c> над <see cref="MemoryExt"/> либо <c>WorldBumpAllocator</c> над основным
	/// аллокатором). <see cref="Create"/> лишь размещает заголовок в уже выделенном блоке.
	///
	/// Следствие-инвариант: <b>структуру нельзя копировать по значению</b>. Обращаться к ней нужно только
	/// через указатель/ссылку в блок — <see cref="SafePtr{T}"/>, <see cref="PtrOffset{T}"/>
	/// (см. <see cref="BumpHeaderExt"/>) или <c>ref</c>. Любое <c>var copy = ptr.Value();</c> с последующим
	/// вызовом метода уведёт <c>&amp;this</c> на стековую копию и тихо повредит память.
	///
	/// Второй инвариант: <b>каждое выданное смещение кратно <see cref="ALIGN"/></b> (см. <see cref="MemAlloc(int)"/>).
	/// Без этого 64-битное поле садится на нечётный офсет, и на 32-битном ARM (ARMv7 в списке
	/// целевых архитектур) его чтение — SIGBUS, а не тихий медленный доступ, как на x64/arm64.
	/// </summary>
	// Size задан явно: HeaderSize обязан быть кратен ALIGN, иначе первая же пользовательская
	// аллокация (корень формата, идущий сразу за заголовком) приезжает на невыровненный офсет.
	[StructLayout(LayoutKind.Sequential, Size = ALIGN * 2)]
	public struct BumpHeader
	{
		/// <summary>
		/// Выравнивание всех выдаваемых смещений. Фиксированная константа, а не <c>TAlign&lt;T&gt;</c>:
		/// раскладка арены обязана совпадать байт-в-байт между выпечкой (редактор, x64) и рантаймом
		/// (arm64/armv7), иначе запечённый дамп читается по чужим смещениям. 8 покрывает все скалярные
		/// типы; SIMD-типов (выравнивание 16) в аренах нет.
		/// </summary>
		public const int ALIGN = 8;

		private int _reservedSize;
		private PtrOffset _rover;

		// Граница на весь блок (не только на сам заголовок) — иначе офсетный доступ к данным
		// упрётся в DEBUG bounds-assert у SafePtr.
		public SafePtr Memory => this.AsSafePtr(_reservedSize);

		/// <summary>
		/// Накладные расходы заголовка в байтах. Обёртка прибавляет их к размеру данных, чтобы понять,
		/// сколько байт выделить под блок (полный размер блока = данные + <see cref="HeaderSize"/>).
		/// </summary>
		public static int HeaderSize => TSize<BumpHeader>.size;

		public int Size => _reservedSize - HeaderSize;

		/// <summary>
		/// Сколько байт блока уже занято (позиция _rover, включая сам заголовок). Read-only — нужно прежде
		/// всего тестам lockstep, чтобы независимо сверить фактический bump с расчётным резервом.
		/// </summary>
		public int UsedBytes => _rover.byteOffset;

		/// <summary>
		/// Размещает заголовок в начале уже выделенного блока <paramref name="memory"/> полным размером
		/// <paramref name="reservedSize"/> байт (включая место под сам заголовок). Память НЕ выделяется —
		/// это делает обёртка.
		/// </summary>
		public static unsafe SafePtr<BumpHeader> Create(SafePtr memory, int reservedSize)
		{
			E.ASSERT(reservedSize > TSize<BumpHeader>.size);
			// Выравнивание смещений внутри арены бесполезно, если сама база блока не выровнена:
			// абсолютный адрес поля = база + смещение.
			E.ASSERT(((ulong)memory.ptr & (ALIGN - 1)) == 0,
				"[BumpHeader] База блока арены не выровнена — обёртка выдала невыровненную память.");

			ref var result = ref memory.Value<BumpHeader>();
			result._reservedSize = reservedSize;
			result._rover = default;
			result.MemAlloc<BumpHeader>(out _) = result;

			return (SafePtr<BumpHeader>)memory;
		}

		/// <summary>
		/// Округляет размер аллокации вверх до <see cref="ALIGN"/>. Расчёт резерва арены обязан
		/// прогонять через это каждую отдельную аллокацию — иначе прогноз занижен и арена переполнится.
		/// </summary>
		public static int Align(int size)
		{
			return MemoryExt.Align(size, ALIGN);
		}

		public PtrOffset MemAlloc(int size)
		{
			E.ASSERT(size > 0);

			// Округляется РАЗМЕР, а не только стартовая позиция: так _rover инвариантно кратен ALIGN,
			// и расход арены становится композиционным — Σ Align(size_i). Именно на это опирается
			// предрасчёт резерва (GetReservedSize / ComputeSizeBreakdown), который иначе разъезжается
			// с фактическим bump на недобитом хвосте последней аллокации.
			var result = new PtrOffset(MemoryExt.Align(_rover.byteOffset, ALIGN));
			_rover = result + Align(size);

			E.ASSERT(_rover.byteOffset <= _reservedSize,
				"[BumpHeader] Арена переполнена: резерв рассчитан без учёта выравнивания аллокаций.");

			return result;
		}

		public PtrOffset MemAlloc(int size, int count)
		{
			return MemAlloc(size * count);
		}

		public PtrOffset<T> MemAlloc<T>()
			where T : unmanaged
		{
			var size = TSize<T>.size;
			return MemAlloc(size);
		}

		public ref T MemAlloc<T>(out PtrOffset<T> offset)
			where T : unmanaged
		{
			var size = TSize<T>.size;
			offset = MemAlloc(size);
			return ref GetValue(offset);
		}

		public ref T MemAlloc<T>(out PtrOffset<T> offset, out SafePtr<T> ptr)
			where T : unmanaged
		{
			var size = TSize<T>.size;
			offset = MemAlloc(size);
			ptr = GetPtr(offset);
			return ref ptr.Value();
		}

		public PtrOffset<T> MemAlloc<T>(int count)
			where T : unmanaged
		{
			var size = TSize<T>.size;
			return MemAlloc(size * count);
		}

		public SafePtr<T> MemAlloc<T>(int count, out PtrOffset<T> offset)
			where T : unmanaged
		{
			var size = TSize<T>.size;
			offset = MemAlloc(size * count);
			return GetPtr(offset);
		}

		public SafePtr GetPtr(PtrOffset offset = default)
		{
			return Memory + offset;
		}

		public SafePtr<T> GetPtr<T>(PtrOffset<T> offset = default)
			where T : unmanaged
		{
			return Memory + offset;
		}

		public ref T GetValue<T>(PtrOffset<T> offset = default)
			where T : unmanaged
		{
			return ref (Memory + offset).Value();
		}

		public ref T GetValue<T>(PtrOffset offset = default)
			where T : unmanaged
		{
			return ref (Memory + offset).Value<T>();
		}

		public void SetupRelativePtr(ref RelativePtr<BumpHeader> relativeOffset)
		{
			relativeOffset.SetPtr(Memory);
		}

		public void Serialize(ref StreamBufferWriter stream)
		{
			stream.Write(_reservedSize);
			stream.Write(Memory, _reservedSize);
			// Мы знаем, что состояние аллокатора сохранено в начале памяти, поэтому ничего больше сериализовать не нужно
		}

		public byte[] SerializeData()
		{
			var span = GetDataSpan();
			var result = new byte[span.Length];
			span.CopyTo(result);
			
			return result;
		}

		/// <summary>
		/// Полезная нагрузка БЕЗ inline-заголовка. Обратно такой span уже не принять: заголовок лежит
		/// в байте 0 блока и без него арену не восстановить — для дампа нужен <see cref="GetBlockSpan"/>.
		/// </summary>
		public Span<byte> GetDataSpan()
		{
			var length = _rover.byteOffset - HeaderSize;
			var from = Memory + HeaderSize;
			return from.GetSpan(length);
		}

		/// <summary>
		/// Блок целиком, включая inline-заголовок — ровно то, что пишет <see cref="Serialize"/> и что
		/// принимает <c>RawBumpAllocator.Deserialize</c>. Это и есть сериализованное представление арены.
		/// </summary>
		public Span<byte> GetBlockSpan()
		{
			return Memory.GetSpan(_reservedSize);
		}

		public void Reset()
		{
			var allocatorSize = TSize<BumpHeader>.size;
			_rover = new PtrOffset(allocatorSize);

			var sizeToReset = _reservedSize - allocatorSize;
			MemoryExt.MemClear(Memory + _rover, sizeToReset);
		}
	}

	/// <summary>
	/// Предполагаем, что аргумент <see cref="offset"/> находится внутри аллокатора.
	/// </summary>
	public static class BumpHeaderExt
	{
		public static PtrOffset MemAlloc(this ref RelativePtr<BumpHeader> relativeOffset, int size)
		{
			return relativeOffset.GetValue().MemAlloc(size);
		}

		public static PtrOffset MemAlloc(this ref RelativePtr<BumpHeader> relativeOffset, int size, int count)
		{
			return relativeOffset.GetValue().MemAlloc(size, count);
		}

		public static PtrOffset<T> MemAlloc<T>(this ref RelativePtr<BumpHeader> relativeOffset)
			where T : unmanaged
		{
			return relativeOffset.GetValue().MemAlloc<T>();
		}

		public static ref T MemAlloc<T>(this ref RelativePtr<BumpHeader> relativeOffset, out PtrOffset<T> offset)
			where T : unmanaged
		{
			return ref relativeOffset.GetValue().MemAlloc(out offset);
		}

		public static PtrOffset<T> MemAlloc<T>(this ref RelativePtr<BumpHeader> relativeOffset, int count)
			where T : unmanaged
		{
			return relativeOffset.GetValue().MemAlloc<T>(count);
		}

		public static SafePtr GetPtr(this ref RelativePtr<BumpHeader> relativeOffset, PtrOffset offset)
		{
			return relativeOffset.GetValue().GetPtr(offset);
		}

		public static SafePtr<T> GetPtr<T>(this ref RelativePtr<BumpHeader> relativeOffset, PtrOffset<T> offset)
			where T : unmanaged
		{
			return relativeOffset.GetValue().GetPtr<T>(offset);
		}

		public static ref T GetRef<T>(this ref RelativePtr<BumpHeader> relativeOffset, PtrOffset<T> offset)
			where T : unmanaged
		{
			return ref relativeOffset.GetValue().GetValue<T>(offset);
		}

		public static ref T GetRef<T>(this ref RelativePtr<BumpHeader> relativeOffset, PtrOffset offset)
			where T : unmanaged
		{
			return ref relativeOffset.GetValue().GetValue<T>(offset);
		}
	}
}
