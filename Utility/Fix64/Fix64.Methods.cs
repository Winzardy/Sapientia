using System;
using System.Collections.Generic;
using UnityEditor;

namespace Sapientia.Deterministic
{
	public partial struct Fix64
	{
		public static int Compare(Fix64 a, Fix64 b)
		{
			return Comparer<long>.Default.Compare(a.m_rawValue, b.m_rawValue);
		}

		public static Fix64 Min(Fix64 a, Fix64 b)
		{
			return a < b ? a : b;
		}

		public static Fix64 Max(Fix64 a, Fix64 b)
		{
			return a > b ? a : b;
		}

		public static Fix64 Clamp(Fix64 value, Fix64 min, Fix64 max)
		{
			return value < min ? min : value > max ? max : value;
		}

		/// <summary>
		/// Note that interpolation is unclamped
		/// and can exceed provided values.
		/// To clamp the result - use LerpClamped instead.
		/// </summary>
		public static Fix64 Lerp(Fix64 a, Fix64 b, Fix64 f)
		{
			return a + f * (b - a);
		}

		public static Fix64 LerpClamped(Fix64 a, Fix64 b, Fix64 f)
		{
			return Lerp(a, b, Clamp01(f));
		}

		public static Fix64 Clamp01(Fix64 value)
		{
			return Clamp(value, Zero, One);
		}

		/// <summary>
		/// 0 - 100
		/// </summary>
		public static Fix64 ClampPercent(Fix64 value)
		{
			return Clamp(value, Zero, 100);
		}

		enum ParserState
		{
			Sign,
			Integer,
			Fractional
		}

		private static readonly long[] POW_10 = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000, 1000000000, 10000000000 };

		public static bool TryParse(string str, out Fix64 value)
		{
			try
			{
				value = Parse(str);
				return true;
			}
			catch
			{
				value = default;
				return false;
			}
		}

		public static Fix64 Parse(string str)
		{
			var state = ParserState.Sign;
			var minus = false;
			var integer = 0L;
			var integerLength = 0;
			var frac = 0L;
			var fracLength = 0;

			for (var pos = 0; pos < str.Length;)
			{
				var ch = str[pos];

				switch (state)
				{
					case ParserState.Sign:
						if (ch == '-')
						{
							minus = true;
							pos++;
						}
						else if (ch == '+')
						{
							pos++;
						}

						state = ParserState.Integer;
						break;

					case ParserState.Integer:
						if (integerLength > 0 && ch == '.')
						{
							state = ParserState.Fractional;
							pos++;
							break;
						}

						if (ch < '0' || ch > '9')
							throw new FormatException($"Invalid character '{ch}' in input string.");

						var digitValue = ch - '0';

						integer = integer * 10 + digitValue;

						pos++;
						integerLength++;

						if (integerLength > 10)
							throw new FormatException("Integer part is too big.");

						break;

					case ParserState.Fractional:
						if (ch < '0' || ch > '9')
							throw new FormatException($"Invalid character '{ch}' in input string.");

						digitValue = ch - '0';

						if (digitValue != 0 || frac != 0)
						{
							frac = frac * 10 + digitValue;
						}

						fracLength++;
						pos++;

						if (fracLength > 10)
							throw new FormatException("Fractional part is too long.");

						break;
				}
			}

			switch (state)
			{
				case ParserState.Integer:
					if (integerLength == 0)
						throw new FormatException("Unexpected end of string.");

					return minus ? -(Fix64)integer : (Fix64)integer;

				case ParserState.Fractional:
					{
						if (fracLength == 0)
							throw new FormatException("Unexpected end of string.");

						var f = FromRaw(integer * ONE + frac * ONE / POW_10[fracLength]);

						return minus ? -f : f;
					}

				default:
					throw new FormatException("Unexpected end of string.");
			}
		}
	}
}
