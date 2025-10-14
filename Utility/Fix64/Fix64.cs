using System;

namespace Sapientia.Deterministic
{
	/// <summary>
	/// Represents a Q31.32 fixed-point number.
	/// </summary>
	[Serializable]
	public partial struct Fix64 : IEquatable<Fix64>, IComparable<Fix64>
	{
#if CLIENT
		[UnityEngine.SerializeField]
		private
#else
		private readonly
#endif
			long m_rawValue;

		// Precision of this type is 2^-32, that is 2,3283064365386962890625E-10
		public static readonly decimal Precision = new Fix64(1L); //0.00000000023283064365386962890625m;
		public static readonly Fix64 MaxValue = new(MAX_VALUE);
		public static readonly Fix64 MinValue = new(MIN_VALUE);
		public static readonly Fix64 One = new(ONE);
		public static readonly Fix64 Zero = new();

		/// <summary>
		/// The value of Pi
		/// </summary>
		public static readonly Fix64 Pi = new(PI);

		public static readonly Fix64 PiOver2 = new(PI_OVER_2);
		public static readonly Fix64 PiTimes2 = new(PI_TIMES_2);
		public static readonly Fix64 PiInv = 0.3183098861837906715377675267M;
		public static readonly Fix64 PiOver2Inv = 0.6366197723675813430755350535M;
		public static readonly Fix64 Log2Max = new(LOG2MAX);
		public static readonly Fix64 Log2Min = new(LOG2MIN);
		public static readonly Fix64 Ln2 = new(LN2);

		internal const long MAX_VALUE = long.MaxValue;
		internal const long MIN_VALUE = long.MinValue;
		internal const int NUM_BITS = 64;
		internal const int FRACTIONAL_PLACES = 32;
		internal const long ONE = 1L << FRACTIONAL_PLACES;
		internal const long PI_TIMES_2 = 0x6487ED511;
		internal const long PI = 0x3243F6A88;
		internal const long PI_OVER_2 = 0x1921FB544;
		internal const long LN2 = 0xB17217F7;
		internal const long LOG2MAX = 0x1F00000000;
		internal const long LOG2MIN = -0x2000000000;

		/// <summary>
		/// The underlying integer representation
		/// </summary>
		public long RawValue => m_rawValue;

		/// <summary>
		/// This is the constructor from raw value; it can only be used interally.
		/// </summary>
		/// <param name="rawValue"></param>
		internal Fix64(long rawValue)
		{
			m_rawValue = rawValue;
		}

		public Fix64(int value)
		{
			m_rawValue = value * ONE;
		}

		public static Fix64 FromRaw(long rawValue)
		{
			return new Fix64(rawValue);
		}

		public static implicit operator Fix64(long value)
		{
			return new Fix64(value * ONE);
		}

		public static implicit operator long(Fix64 value)
		{
			return value.m_rawValue >> FRACTIONAL_PLACES;
		}

		public static implicit operator Fix64(float value)
		{
			return new Fix64((long) (value * ONE));
		}

		public static implicit operator float(Fix64 value)
		{
			return (float) value.m_rawValue / ONE;
		}

		public static implicit operator Fix64(double value)
		{
			return new Fix64((long) (value * ONE));
		}

		public static implicit operator double(Fix64 value)
		{
			return (double) value.m_rawValue / ONE;
		}

		public static implicit operator Fix64(decimal value)
		{
			return new Fix64((long) (value * ONE));
		}

		public static implicit operator decimal(Fix64 value)
		{
			return (decimal) value.m_rawValue / ONE;
		}

		public override bool Equals(object? obj)
		{
			return obj is Fix64 fix64 && fix64.m_rawValue == m_rawValue;
		}

		public bool Equals(Fix64 other)
		{
			return m_rawValue == other.m_rawValue;
		}

		public int CompareTo(Fix64 other)
		{
			return m_rawValue.CompareTo(other.m_rawValue);
		}

		public override string ToString()
		{
			// Up to 10 decimal places
			return ToString("0.##########");
		}

		public string ToString(string format)
		{
			// Up to 10 decimal places
			return ((decimal) this).ToString(format);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return ((decimal) this).ToString(format, formatProvider);
		}

		public override int GetHashCode()
		{
			return m_rawValue.GetHashCode();
		}
	}
}
