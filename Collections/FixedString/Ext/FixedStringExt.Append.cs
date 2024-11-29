using System;
using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.Collections.Fixed
{
	/// <summary>
	/// Provides extension methods for FixedString*N*Bytes.
	/// </summary>
#if BURST
	[Unity.Collections.BurstCompatible]
#endif
	public static unsafe partial class FixedStringExt
	{
		[StructLayout(LayoutKind.Explicit)]
		internal struct UintFloatUnion
		{
			[FieldOffset(0)] public uint uintValue;
			[FieldOffset(0)] public float floatValue;
		}

		internal static ParseError Base10ToBase2(ref float output, ulong mantissa10, int exponent10)
		{
			if (mantissa10 == 0)
			{
				output = 0.0f;
				return ParseError.None;
			}

			if (exponent10 == 0)
			{
				output = mantissa10;
				return ParseError.None;
			}

			var exponent2 = exponent10;
			var mantissa2 = mantissa10;
			while (exponent10 > 0)
			{
				while ((mantissa2 & 0xe000000000000000U) != 0)
				{
					mantissa2 >>= 1;
					++exponent2;
				}

				mantissa2 *= 5;
				--exponent10;
			}

			while (exponent10 < 0)
			{
				while ((mantissa2 & 0x8000000000000000U) == 0)
				{
					mantissa2 <<= 1;
					--exponent2;
				}

				mantissa2 /= 5;
				++exponent10;
			}

			// TODO: implement math.ldexpf (which presumably handles denormals (i don't))
			UintFloatUnion ufu = new UintFloatUnion();
			ufu.floatValue = mantissa2;
			var e = (int)((ufu.uintValue >> 23) & 0xFFU) - 127;
			e += exponent2;
			if (e > 128)
				return ParseError.Overflow;
			if (e < -127)
				return ParseError.Underflow;
			ufu.uintValue = (ufu.uintValue & ~(0xFFU << 23)) | ((uint)(e + 127) << 23);
			output = ufu.floatValue;
			return ParseError.None;
		}

		internal static void Base2ToBase10(ref ulong mantissa10, ref int exponent10, float input)
		{
			UintFloatUnion ufu = new UintFloatUnion();
			ufu.floatValue = input;
			if (ufu.uintValue == 0)
			{
				mantissa10 = 0;
				exponent10 = 0;
				return;
			}

			var mantissa2 = (ufu.uintValue & ((1 << 23) - 1)) | (1 << 23);
			var exponent2 = (int)(ufu.uintValue >> 23) - 127 - 23;
			mantissa10 = mantissa2;
			exponent10 = exponent2;
			if (exponent2 > 0)
			{
				while (exponent2 > 0)
				{
					// denormalize mantissa10 as much as you can, to minimize loss when doing /5 below.
					while (mantissa10 <= UInt64.MaxValue / 10)
					{
						mantissa10 *= 10;
						--exponent10;
					}

					mantissa10 /= 5;
					--exponent2;
				}
			}

			if (exponent2 < 0)
			{
				while (exponent2 < 0)
				{
					// normalize mantissa10 just as much as you need, in order to make the *5 below not overflow.
					while (mantissa10 > UInt64.MaxValue / 5)
					{
						mantissa10 /= 10;
						++exponent10;
					}

					mantissa10 *= 5;
					++exponent2;
				}
			}

			// normalize mantissa10
			while (mantissa10 > 9999999U || mantissa10 % 10 == 0)
			{
				mantissa10 = (mantissa10 + (mantissa10 < 100000000U ? 5u : 0u)) / 10;
				++exponent10;
			}
		}

		/// <summary>
		/// Appends a Unicode.Rune to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="rune">A Unicode.Rune to append.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, Unicode.Rune rune)
			where T : struct, IFixedString
		{
			var len = fs.Length;
			var runeLen = rune.LengthInUtf8Bytes();
			if (!fs.TryResize(len + runeLen, ClearOptions.UninitializedMemory))
				return FormatError.Overflow;
			return fs.Write(ref len, rune);
		}

		/// <summary>
		/// Appends a char to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="ch">A char to append.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, char ch)
			where T : struct, IFixedString
		{
			return fs.Append((Unicode.Rune)ch);
		}

		/// <summary>
		/// Appends a byte to this string.
		/// </summary>
		/// <remarks>
		/// No validation is performed: it is your responsibility for the data to be valid UTF-8 when you're done appending bytes.
		/// </remarks>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="a">A byte to append.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError AppendRawByte<T>(ref this T fs, byte a)
			where T : struct, IFixedString
		{
			var origLength = fs.Length;
			if (!fs.TryResize(origLength + 1, ClearOptions.UninitializedMemory))
				return FormatError.Overflow;
			fs.GetUnsafePtr()[origLength] = a;
			return FormatError.None;
		}

		/// <summary>
		/// Appends a Unicode.Rune a number of times to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="rune">A Unicode.Rune to append some number of times.</param>
		/// <param name="count">The number of times to append the rune.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, Unicode.Rune rune, int count)
			where T : struct, IFixedString
		{
			var origLength = fs.Length;

			if (!fs.TryResize(origLength + rune.LengthInUtf8Bytes() * count, ClearOptions.UninitializedMemory))
				return FormatError.Overflow;

			var cap = fs.Capacity;
			var b = fs.GetUnsafePtr();
			int offset = origLength;
			for (int i = 0; i < count; ++i)
			{
				var error = Unicode.UcsToUtf8(b, ref offset, cap, rune);
				if (error != ConversionError.None)
					return FormatError.Overflow;
			}

			return FormatError.None;
		}

		/// <summary>
		/// Appends a number (converted to UTF-8 characters) to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="input">A long integer to append to the string.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, long input)
			where T : struct, IFixedString
		{
			const int maximumDigits = 20;
			var temp = stackalloc byte[maximumDigits];
			int offset = maximumDigits;
			if (input >= 0)
			{
				do
				{
					var digit = (byte)(input % 10);
					temp[--offset] = (byte)('0' + digit);
					input /= 10;
				} while (input != 0);
			}
			else
			{
				do
				{
					var digit = (byte)(input % 10);
					temp[--offset] = (byte)('0' - digit);
					input /= 10;
				} while (input != 0);

				temp[--offset] = (byte)'-';
			}

			return fs.Append(temp + offset, maximumDigits - offset);
		}

		/// <summary>
		/// Appends a number (converted to UTF-8 characters) to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="input">An int to append to the string.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, int input)
			where T : struct, IFixedString
		{
			return fs.Append((long)input);
		}

		/// <summary>
		/// Appends a number (converted to UTF-8 characters) to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="input">A ulong integer to append to the string.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, ulong input)
			where T : struct, IFixedString
		{
			const int maximumDigits = 20;
			var temp = stackalloc byte[maximumDigits];
			int offset = maximumDigits;
			do
			{
				var digit = (byte)(input % 10);
				temp[--offset] = (byte)('0' + digit);
				input /= 10;
			} while (input != 0);

			return fs.Append(temp + offset, maximumDigits - offset);
		}

		/// <summary>
		/// Appends a number (converted to UTF-8 characters) to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="input">A uint to append to the string.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, uint input)
			where T : struct, IFixedString
		{
			return fs.Append((ulong)input);
		}

		/// <summary>
		/// Appends a number (converted to UTF-8 characters) to this string.
		/// </summary>
		/// <typeparam name="T">The type of FixedString*N*Bytes.</typeparam>
		/// <param name="fs">A FixedString*N*Bytes.</param>
		/// <param name="input">A float to append to the string.</param>
		/// <param name="decimalSeparator">The character to use as the decimal separator. Defaults to a period ('.').</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T>(ref this T fs, float input, char decimalSeparator = '.')
			where T : struct, IFixedString
		{
			UintFloatUnion ufu = new UintFloatUnion();
			ufu.floatValue = input;
			var sign = ufu.uintValue >> 31;
			ufu.uintValue &= ~(1 << 31);
			FormatError error;
			if ((ufu.uintValue & 0x7F800000) == 0x7F800000)
			{
				if (ufu.uintValue == 0x7F800000)
				{
					if (sign != 0 && ((error = fs.Append('-')) != FormatError.None))
						return error;
					return fs.Append('I', 'n', 'f', 'i', 'n', 'i', 't', 'y');
				}

				return fs.Append('N', 'a', 'N');
			}

			if (sign != 0 && ufu.uintValue != 0) // C# prints -0 as 0
				if ((error = fs.Append('-')) != FormatError.None)
					return error;
			ulong decimalMantissa = 0;
			int decimalExponent = 0;
			Base2ToBase10(ref decimalMantissa, ref decimalExponent, ufu.floatValue);
			var backwards = stackalloc char[9];
			int decimalDigits = 0;
			do
			{
				if (decimalDigits >= 9)
					return FormatError.Overflow;
				var decimalDigit = decimalMantissa % 10;
				backwards[8 - decimalDigits++] = (char)('0' + decimalDigit);
				decimalMantissa /= 10;
			} while (decimalMantissa > 0);

			char* ascii = backwards + 9 - decimalDigits;
			var leadingZeroes = -decimalExponent - decimalDigits + 1;
			if (leadingZeroes > 0)
			{
				if (leadingZeroes > 4)
					return fs.AppendScientific(ascii, decimalDigits, decimalExponent, decimalSeparator);
				if ((error = fs.Append('0', decimalSeparator)) != FormatError.None)
					return error;
				--leadingZeroes;
				while (leadingZeroes > 0)
				{
					if ((error = fs.Append('0')) != FormatError.None)
						return error;
					--leadingZeroes;
				}

				for (var i = 0; i < decimalDigits; ++i)
				{
					if ((error = fs.Append(ascii[i])) != FormatError.None)
						return error;
				}

				return FormatError.None;
			}

			var trailingZeroes = decimalExponent;
			if (trailingZeroes > 0)
			{
				if (trailingZeroes > 4)
					return fs.AppendScientific(ascii, decimalDigits, decimalExponent, decimalSeparator);
				for (var i = 0; i < decimalDigits; ++i)
				{
					if ((error = fs.Append(ascii[i])) != FormatError.None)
						return error;
				}

				while (trailingZeroes > 0)
				{
					if ((error = fs.Append('0')) != FormatError.None)
						return error;
					--trailingZeroes;
				}

				return FormatError.None;
			}

			var indexOfSeparator = decimalDigits + decimalExponent;
			for (var i = 0; i < decimalDigits; ++i)
			{
				if (i == indexOfSeparator)
					if ((error = fs.Append(decimalSeparator)) != FormatError.None)
						return error;
				if ((error = fs.Append(ascii[i])) != FormatError.None)
					return error;
			}

			return FormatError.None;
		}

		/// <summary>
		/// Appends another string to this string.
		/// </summary>
		/// <remarks>
		/// When the method returns an error, the destination string is not modified.
		/// </remarks>
		/// <typeparam name="T">The type of the destination string.</typeparam>
		/// <typeparam name="T2">The type of the source string.</typeparam>
		/// <param name="fs">The destination string.</param>
		/// <param name="input">The source string.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the destination string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes), typeof(FixedString128Bytes) })]
#endif
		public static FormatError Append<T, T2>(ref this T fs, ref T2 input)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.Append(input.GetUnsafePtr(), input.Length);
		}

		/// <summary>
		/// Copies another string to this string (making the two strings equal).
		/// </summary>
		/// <remarks>
		/// When the method returns an error, the destination string is not modified.
		/// </remarks>
		/// <typeparam name="T">The type of the destination string.</typeparam>
		/// <typeparam name="T2">The type of the source string.</typeparam>
		/// <param name="fs">The destination string.</param>
		/// <param name="input">The source string.</param>
		/// <returns>CopyError.None if successful. Returns CopyError.Truncation if the source string is too large to fit in the destination.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes), typeof(FixedString128Bytes) })]
#endif
		public static CopyError CopyFrom<T, T2>(ref this T fs, ref T2 input)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			fs.Length = 0;
			var fe = Append(ref fs, ref input);
			if (fe != FormatError.None)
				return CopyError.Truncation;
			return CopyError.None;
		}

		/// <summary>
		/// Appends bytes to this string.
		/// </summary>
		/// <remarks>
		/// When the method returns an error, the destination string is not modified.
		///
		/// No validation is performed: it is your responsibility for the destination to contain valid UTF-8 when you're done appending bytes.
		/// </remarks>
		/// <typeparam name="T">The type of the destination string.</typeparam>
		/// <param name="fs">The destination string.</param>
		/// <param name="utf8Bytes">The bytes to append.</param>
		/// <param name="utf8BytesLength">The number of bytes to append.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the destination string is exceeded.</returns>
#if BURST
		[Unity.Collections.BurstCompatible(GenericTypeArguments = new[] { typeof(FixedString128Bytes) })]
#endif
#if UNITY_5_3_OR_NEWER
		public unsafe static FormatError Append<T>(ref this T fs, byte* utf8Bytes, int utf8BytesLength)
			where T : struct, IFixedString
		{
			var origLength = fs.Length;
			if (!fs.TryResize(origLength + utf8BytesLength, ClearOptions.UninitializedMemory))
				return FormatError.Overflow;
			Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(fs.GetUnsafePtr() + origLength, utf8Bytes, utf8BytesLength);
			return FormatError.None;
		}
#else
		public unsafe static FormatError Append<T>(ref this T fs, byte* utf8Bytes, int utf8BytesLength)
			where T : struct, IFixedString
		{
			var origLength = fs.Length;
			if (!fs.TryResize(origLength + utf8BytesLength, ClearOptions.UninitializedMemory))
				return FormatError.Overflow;
			Buffer.MemoryCopy(utf8Bytes, fs.GetUnsafePtr() + origLength, utf8BytesLength, utf8BytesLength);
			return FormatError.None;
		}
#endif

		/// <summary>
		/// Appends another string to this string.
		/// </summary>
		/// <remarks>
		/// When the method returns an error, the destination string is not modified.
		/// </remarks>
		/// <typeparam name="T">The type of the destination string.</typeparam>
		/// <param name="fs">The destination string.</param>
		/// <param name="s">The string to append.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the capacity of the destination string is exceeded.</returns>
#if BURST
		[Unity.Collections.NotBurstCompatible]
#endif
		public unsafe static FormatError Append<T>(ref this T fs, string s)
			where T : struct, IFixedString
		{
			// we don't know how big the expansion from UTF16 to UTF8 will be, so we account for worst case.
			int worstCaseCapacity = s.Length * 4;
			byte* utf8Bytes = stackalloc byte[worstCaseCapacity];
			int utf8Len;

			fixed (char* chars = s)
			{
				var err = UTF8Ext.Copy(utf8Bytes, out utf8Len, worstCaseCapacity, chars, s.Length);
				if (err != CopyError.None)
				{
					return FormatError.Overflow;
				}
			}

			return fs.Append(utf8Bytes, utf8Len);
		}

		/// <summary>
		/// Copies another string to this string (making the two strings equal).
		/// </summary>
		/// <remarks>
		/// When the method returns an error, the destination string is not modified.
		/// </remarks>
		/// <typeparam name="T">The type of the destination string.</typeparam>
		/// <param name="fs">The destination string.</param>
		/// <param name="s">The source string.</param>
		/// <returns>CopyError.None if successful. Returns CopyError.Truncation if the source string is too large to fit in the destination.</returns>
#if BURST
		[Unity.Collections.NotBurstCompatible]
#endif
		public static CopyError CopyFrom<T>(ref this T fs, string s)
			where T : struct, IFixedString
		{
			fs.Length = 0;
			var fe = Append(ref fs, s);
			if (fe != FormatError.None)
				return CopyError.Truncation;
			return CopyError.None;
		}

		/// <summary>
		/// Copies another string to this string. If the string exceeds the capacity it will be truncated.
		/// </summary>
		/// <typeparam name="T">The type of the destination string.</typeparam>
		/// <param name="fs">The destination string.</param>
		/// <param name="s">The source string.</param>
#if BURST
		[Unity.Collections.NotBurstCompatible]
#endif
		public static void CopyFromTruncated<T>(ref this T fs, string s)
			where T : struct, IFixedString
		{
			int utf8Len;
			fixed (char* chars = s)
			{
				UTF8Ext.Copy(fs.GetUnsafePtr(), out utf8Len, fs.Capacity, chars, s.Length);
				fs.Length = utf8Len;
			}
		}
	}
}
