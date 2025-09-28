using System;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	[Serializable]
	public struct ByteEnumMask<T>
#if UNITY_EDITOR
		: IEnumMask
#endif
		where T : unmanaged, Enum
	{
		public const int BitsCount = 8;

		public static readonly ByteEnumMask<T> All = new() { mask = unchecked((byte)(~0)) };

		public byte mask;

		private static void AssertValue(byte value)
		{
			E.ASSERT(value is < BitsCount and >= 0, $"Количество бит выходит за пределы рабочего диапазона [0 >= (value: {value}) < {BitsCount}]");
		}

		public ByteEnumMask(params T[] values) : this()
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
			return Has(value.ToByte());
		}

		public readonly bool HasOnly(T value)
		{
			return HasOnly(value.ToByte());
		}

		public readonly bool Has(byte value)
		{
			AssertValue(value);
			return (mask & (1 << value)) != 0;
		}

		public readonly bool HasOnly(byte value)
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
			Add(value.ToByte());
		}

		public void Add(byte value)
		{
			AssertValue(value);
			mask |= (byte)(1 << value);
		}

		public void Remove(T value)
		{
			Remove(value.ToByte());
		}

		public void Remove(byte value)
		{
			AssertValue(value);
			mask &= (byte)~(1 << value);
		}

		public static ByteEnumMask<T> operator |(ByteEnumMask<T> a, ByteEnumMask<T> b)
		{
			return new ByteEnumMask<T> { mask = (byte)(a.mask | b.mask) };
		}

		public static EnumMask<T> operator &(ByteEnumMask<T> a, ByteEnumMask<T> b)
		{
			return new EnumMask<T> { mask = a.mask & b.mask };
		}

		public static implicit operator ByteEnumMask<T>(T value)
		{
			var mask = new ByteEnumMask<T>();
			mask.Add(value);
			return mask;
		}

		public static implicit operator byte(ByteEnumMask<T> value)
		{
			return value.mask;
		}

		public static implicit operator ByteEnumMask<T>(byte value)
		{
			return new ByteEnumMask<T>
			{
				mask = value,
			};
		}
	}
}
