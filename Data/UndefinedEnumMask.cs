using System;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public struct UndefinedEnumMask
	{
		public const int BitsCount = 32;

		public static readonly UndefinedEnumMask All = new() { mask = ~0 };

		public int mask;

		public static UndefinedEnumMask Create<T>(T value)
			where T: unmanaged, Enum
		{
			var result = new UndefinedEnumMask();
			result.Add(value);
			return result;
		}

		public static UndefinedEnumMask Union<T1, T2>(EnumMask<T1> a, EnumMask<T2> b)
			where T1 : unmanaged, Enum
			where T2 : unmanaged, Enum
		{
			var shift = EnumValues<T1>.ENUM_LENGHT;
			var length = shift + EnumValues<T2>.ENUM_LENGHT;

			E.ASSERT(length is < BitsCount and >= 0);

			var mask = (a.mask | (b.mask << shift)).TruncateBits(length);
			return new UndefinedEnumMask
			{
				mask = mask,
			};
		}

		public bool HasNothing()
		{
			return mask == 0;
		}

		public readonly bool Has<T>(T value)
			where T: unmanaged, Enum
		{
			return Has(value.ToInt());
		}

		public readonly bool HasOnly<T>(T value)
			where T: unmanaged, Enum
		{
			return HasOnly(value.ToInt());
		}

		public readonly bool Has(int value)
		{
			E.ASSERT(value is < BitsCount and >= 0);
			return (mask & (1 << value)) != 0;
		}

		public readonly bool HasOnly(int value)
		{
			E.ASSERT(value is < BitsCount and >= 0);
			var valueMask = 1 << value;
			return ((mask & (1 << value)) != 0) && ((mask & ~valueMask) == 0);
		}

		public readonly bool Has(UndefinedEnumMask value)
		{
			return (value.mask & mask) != 0;
		}

		public void Add<T>(T value)
			where T: unmanaged, Enum
		{
			Add(value.ToInt());
		}

		public void Add(int value)
		{
			E.ASSERT(value is < BitsCount and >= 0);
			mask |= (1 << value);
		}

		public void Remove<T>(T value)
			where T: unmanaged, Enum
		{
			Remove(value.ToInt());
		}

		public void Remove(int value)
		{
			E.ASSERT(value is < BitsCount and >= 0);
			mask &= ~(1 << value);
		}

		public static UndefinedEnumMask operator |(UndefinedEnumMask a, UndefinedEnumMask b)
		{
			return new UndefinedEnumMask {mask = a.mask | b.mask};
		}

		public static UndefinedEnumMask operator &(UndefinedEnumMask a, UndefinedEnumMask b)
		{
			return new UndefinedEnumMask {mask = a.mask & b.mask};
		}

		public static implicit operator uint(UndefinedEnumMask value)
		{
			return value.mask.As<int, uint>();
		}

		public static implicit operator UndefinedEnumMask(uint value)
		{
			var mask = value.As<uint, int>();
			return new UndefinedEnumMask
			{
				mask = mask,
			};
		}
	}
}
