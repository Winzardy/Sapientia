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
		public static readonly ByteEnumMask<T> All = new() { mask = unchecked((byte)(~0)) };

		public byte mask;

		public static ByteEnumMask<T> Create(T value)
		{
			var result = new ByteEnumMask<T>();
			result.Add(value);
			return result;
		}

		public bool HasNothing()
		{
			return mask == 0;
		}

		public readonly bool Has(T value)
		{
			return Has(value.ToInt());
		}

		public readonly bool HasOnly(T value)
		{
			return HasOnly(value.ToInt());
		}

		public readonly bool Has(int value)
		{
			return (mask & (1 << value)) != 0;
		}

		public readonly bool HasOnly(int value)
		{
			var valueMask = 1 << value;
			return ((mask & (1 << value)) != 0) && ((mask & ~valueMask) == 0);
		}

		public readonly bool Has(EnumMask<T> value)
		{
			return (value.mask & mask) != 0;
		}

		public void Add(T value)
		{
			Add(value.ToByte());
		}

		public void Add(byte value)
		{
			mask |= (byte)(1 << value);
		}

		public void Remove(T value)
		{
			Remove(value.ToByte());
		}

		public void Remove(byte value)
		{
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

	public class ByteEnumMask
	{
		public static ByteEnumMask<T> From<T>(params T[] values)
			where T : unmanaged, Enum
		{
			var mask = new ByteEnumMask<T>();

			for (var i = 0; i < values.Length; i++)
				mask.Add(values[i]);

			return mask;
		}
	}
}
