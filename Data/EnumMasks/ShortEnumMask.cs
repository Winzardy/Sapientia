using System;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	[Serializable]
	public struct ShortEnumMask<T>
#if UNITY_EDITOR
		: IEnumMask
#endif
		where T : unmanaged, Enum
	{
		public const int BitsCount = 16;

		public static readonly ShortEnumMask<T> All = new() { mask = unchecked((short)(~0)) };

		public short mask;

		private static void AssertValue(short value)
		{
			E.ASSERT(value is < BitsCount and >= 0, $"Количество бит выходит за пределы рабочего диапазона [0 >= (value: {value}) < {BitsCount}]");
		}

		public ShortEnumMask(params T[] values) : this()
		{
			for (var i = 0; i < values.Length; i++)
				Add(values[i]);
		}

		public bool HasNothing()
		{
			return mask == 0;
		}

		public readonly bool Has(T value)
		{
			return Has(value.ToShort());
		}

		public readonly bool HasOnly(T value)
		{
			return HasOnly(value.ToShort());
		}

		public readonly bool Has(short value)
		{
			AssertValue(value);
			return (mask & (1 << value)) != 0;
		}

		public readonly bool HasOnly(short value)
		{
			AssertValue(value);
			var valueMask = 1 << value;
			return ((mask & (1 << value)) != 0) && ((mask & ~valueMask) == 0);
		}

		public readonly bool Has(EnumMask<T> value)
		{
			return (value.mask & mask) != 0;
		}

		public void Set(T value, bool add)
		{
			if (add)
				Add(value);
			else
				Remove(value);
		}

		public void Add(T value)
		{
			Add(value.ToShort());
		}

		public void Add(short value)
		{
			AssertValue(value);
			mask |= (short)(1 << value);
		}

		public void Remove(T value)
		{
			Remove(value.ToShort());
		}

		public void Remove(short value)
		{
			AssertValue(value);
			mask &= (short)~(1 << value);
		}

		public static ShortEnumMask<T> operator |(ShortEnumMask<T> a, ShortEnumMask<T> b)
		{
			return new ShortEnumMask<T> { mask = (short)(a.mask | b.mask) };
		}

		public static EnumMask<T> operator &(ShortEnumMask<T> a, ShortEnumMask<T> b)
		{
			return new EnumMask<T> { mask = a.mask & b.mask };
		}

		public static implicit operator ShortEnumMask<T>(T value)
		{
			var mask = new ShortEnumMask<T>();
			mask.Add(value);
			return mask;
		}

		public static implicit operator short(ShortEnumMask<T> value)
		{
			return value.mask;
		}

		public static implicit operator ShortEnumMask<T>(short value)
		{
			return new ShortEnumMask<T>
			{
				mask = value,
			};
		}
	}
}
