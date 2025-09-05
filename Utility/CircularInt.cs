using System;

namespace Sapientia.Utility
{
	public struct CircularInt
	{
		private int _value;
		private readonly int _maxValue;

		public int Value => _value;
		public int MaxValue => _maxValue;


		public CircularInt(int maxValue, int initialValue = 0)
		{
			if (maxValue <= 0)
				throw new ArgumentException("The maximum value must be greater than zero");

			_maxValue = maxValue;
			_value = initialValue % maxValue;
		}

		public static CircularInt operator ++(CircularInt c)
		{
			c._value = (c._value + 1) % c._maxValue;
			return c;
		}

		public static CircularInt operator --(CircularInt c)
		{
			c._value = (c._value - 1 + c._maxValue) % c._maxValue;
			return c;
		}

		public static implicit operator int(CircularInt c)
		{
			return c._value;
		}

		public override string ToString()
		{
			return _value.ToString();
		}
	}
}
