using System;
using Sapientia.Extensions;

namespace Sapientia.Data
{
#if UNITY_EDITOR
	public interface IEnumMask {}
#endif

	[Serializable]
	public struct EnumMask<T>
#if UNITY_EDITOR
		: IEnumMask
#endif
		where T : unmanaged, Enum
	{
		public static readonly EnumMask<T> All = new() { mask = ~0 };

		public int mask;

		public bool HasNothing()
		{
			return mask == 0;
		}

		public readonly bool Has(T value)
		{
			return Has(value.ToInt());
		}

		public readonly bool Has(int value)
		{
			return (mask & (1 << value)) != 0;
		}

		public readonly bool Has(EnumMask<T> value)
		{
			return (value.mask & mask) != 0;
		}

		public void Add(T value)
		{
			Add(value.ToInt());
		}

		public void Add(int value)
		{
			mask |= (1 << value);
		}

		public void Remove(T value)
		{
			Remove(value.ToInt());
		}

		public void Remove(int value)
		{
			mask &= ~(1 << value);
		}

		public static EnumMask<T> operator |(EnumMask<T> a, EnumMask<T> b)
		{
			return new EnumMask<T> { mask = a.mask | b.mask };
		}

		public static EnumMask<T> operator &(EnumMask<T> a, EnumMask<T> b)
		{
			return new EnumMask<T> { mask = a.mask & b.mask };
		}

		public static implicit operator EnumMask<T>(T value)
		{
			var mask = new EnumMask<T>();
			mask.Add(value);
			return mask;
		}

		public static implicit operator uint(EnumMask<T> value)
		{
			return value.mask.As<int, uint>();
		}
	}
}