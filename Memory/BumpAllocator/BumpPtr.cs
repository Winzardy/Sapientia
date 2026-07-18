using Sapientia.Data;

namespace Sapientia.Memory
{
	/// <summary>
	/// Типизированный хэндл на значение в bump-арене: абсолютный указатель на заголовок + смещение данных.
	/// В отличие от inline-заголовка <see cref="BumpHeader"/> и self-relative контейнеров
	/// (<see cref="BumpArray{T}"/>/<see cref="BumpDictionary{TKey,TValue}"/>) хэндл МОЖНО копировать
	/// по значению — внутри нет self-relative полей, копия указывает на те же данные.
	///
	/// Хэндл рантаймовый: абсолютный указатель не сериализуется вместе с ареной и валиден, только пока
	/// блок арены жив и неподвижен (путь <see cref="RawBumpAllocator"/>). Для перемещаемых блоков поверх
	/// основного аллокатора храните <see cref="PtrOffset{T}"/> и резолвьте заново после переезда.
	/// </summary>
	public readonly struct BumpPtr<T> where T : unmanaged
	{
		private readonly SafePtr<BumpHeader> _header;
		private readonly PtrOffset<T> _offset;

		public bool IsValid => _header.IsValid && _offset.isValid;

		public ref T Value => ref _header.Value().GetValue(_offset);

		public BumpPtr(SafePtr<BumpHeader> header, PtrOffset<T> offset)
		{
			_header = header;
			_offset = offset;
		}

		public SafePtr<T> GetPtr()
		{
			return _header.Value().GetPtr(_offset);
		}
	}
}
