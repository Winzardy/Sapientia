using Sapientia.Data;
using Sapientia.Extensions;

// Copy of Unity.Collections.Unicode file
namespace Sapientia.Collections.FixedString
{
	/// <summary>
	/// Kinds of format errors.
	/// </summary>
	public enum FormatError
	{
		/// <summary>
		/// No error.
		/// </summary>
		None,

		/// <summary>
		/// The target storage does not have sufficient capacity.
		/// </summary>
		Overflow,
	}

	/// <summary>
	/// Kinds of parse errors.
	/// </summary>
	public enum ParseError
	{
		/// <summary>
		/// No parse error.
		/// </summary>
		None,

		/// <summary>
		/// The text parsed does not form a number.
		/// </summary>
		Syntax,

		/// <summary>
		/// The number exceeds the range of the target type.
		/// </summary>
		Overflow,

		/// <summary>
		/// The number exceeds the precision of the target type.
		/// </summary>
		Underflow,
	}

	/// <summary>
	/// Kinds of copy errors.
	/// </summary>
	public enum CopyError
	{
		/// <summary>
		/// No copy error.
		/// </summary>
		None,

		/// <summary>
		/// The target storage does not have sufficient capacity.
		/// </summary>
		Truncation,
	}

	/// <summary>
	/// Kinds of conversion errors.
	/// </summary>
	public enum ConversionError
	{
		/// <summary>
		/// No conversion error.
		/// </summary>
		None,

		/// <summary>
		/// The target storage does not have sufficient capacity.
		/// </summary>
		Overflow,

		/// <summary>
		/// The bytes do not form a valid character.
		/// </summary>
		Encoding,

		/// <summary>
		/// The rune is not a valid code point.
		/// </summary>
		CodePoint,
	}

	/// <summary>
	/// Provides utility methods for UTF-8, UTF-16, UCS-4 (a.k.a. UTF-32), and WTF-8.
	/// </summary>

	public unsafe struct Unicode
	{
		/// <summary>
		/// Representation of a Unicode character as a code point.
		/// </summary>
#if BURST
		[Unity.Collections.BurstCompatible]
#endif
		public struct Rune
		{
			/// <summary>
			/// The code point.
			/// </summary>
			/// <value>The code point.</value>
			public int value;

			/// <summary>
			/// Initializes and returns an instance of Rune.
			/// </summary>
			/// <remarks>You are responsible for the code point being valid.</remarks>
			/// <param name="codepoint">The code point.</param>
			public Rune(int codepoint)
			{
				value = codepoint;
			}

			/// <summary>
			/// Returns a rune.
			/// </summary>
			/// <remarks>Because a char is 16-bit, it can only represent the first 2^16 code points, not all 1.1 million.</remarks>
			/// <param name="codepoint">A code point.</param>
			/// <returns>A rune.</returns>
			public static explicit operator Rune(char codepoint) => new Rune { value = codepoint };

			/// <summary>
			/// Returns true if a rune is a numerical digit character.
			/// </summary>
			/// <param name="r">The rune.</param>
			/// <returns>True if the rune is a numerical digit character.</returns>
			public static bool IsDigit(Rune r)
			{
				return r.value >= '0' && r.value <= '9';
			}

			/// <summary>
			/// Returns the number of bytes required to encode this rune as UTF-8.
			/// </summary>
			/// <returns>The number of bytes required to encode this rune as UTF-8. If the rune's codepoint
			/// is invalid, returns 4 (the maximum possible encoding length).</returns>
			public int LengthInUtf8Bytes()
			{
				if (value < 0)
					return 4; // invalid codepoint
				if (value <= 0x7F)
					return 1;
				if (value <= 0x7FF)
					return 2;
				if (value <= 0xFFFF)
					return 3;
				if (value <= 0x1FFFFF)
					return 4;
				// invalid codepoint, max size.
				return 4;
			}
		}

		/// <summary>The maximum value of a valid UNICODE code point</summary>
		public const int kMaximumValidCodePoint = 0x10FFFF;

		/// <summary>
		/// Returns true if a code point is valid.
		/// </summary>
		/// <param name="codepoint">A code point.</param>
		/// <returns>True if a code point is valid.</returns>
		public static bool IsValidCodePoint(int codepoint)
		{
			if (codepoint > kMaximumValidCodePoint) // maximum valid code point
				return false;
//            if (codepoint >= 0xD800 && codepoint <= 0xDFFF) // surrogate pair
//                return false;
			if (codepoint < 0) // negative?
				return false;
			return true;
		}

		/// <summary>
		/// Returns true if the byte is not the last byte of a UTF-8 character.
		/// </summary>
		/// <param name="b">The byte.</param>
		/// <returns>True if the byte is not the last byte of a UTF-8 character.</returns>
		public static bool NotTrailer(byte b)
		{
			return (b & 0xC0) != 0x80;
		}

		/// <summary>
		/// The Unicode character �.
		/// </summary>
		/// <remarks>This character is used to stand-in for characters that can't be rendered.</remarks>
		/// <value>The Unicode character �.</value>
		public static Rune ReplacementCharacter => new Rune { value = 0xFFFD };

		/// <summary>
		/// The null rune value.
		/// </summary>
		/// <remarks>In this package, the "bad rune" is used as a null character. It represents no valid code point.</remarks>
		/// <value>The null rune value.</value>
		public static Rune BadRune => new Rune { value = 0 };

		/// <summary>
		/// Reads a UTF-8 encoded character from a buffer.
		/// </summary>
		/// <param name="rune">Outputs the character read. If the read fails, outputs <see cref="ReplacementCharacter"/>.</param>
		/// <param name="buffer">The buffer of bytes to read.</param>
		/// <param name="index">Reference to a byte index into the buffer. If the read succeeds, index is incremented by the
		/// size in bytes of the character read. If the read fails, index is incremented by 1.</param>
		/// <param name="capacity">The size in bytes of the buffer. Used to check that the read is in bounds.</param>
		/// <returns><see cref="ConversionError.None"/> if the read succeeds. Otherwise, returns <see cref="ConversionError.Overflow"/> or <see cref="ConversionError.Encoding"/>.</returns>
		public static ConversionError Utf8ToUcs(out Rune rune, SafePtr buffer, ref int index, int capacity)
		{
			int code = 0;
			rune = ReplacementCharacter;
			if (index + 1 > capacity)
			{
				return ConversionError.Overflow;
			}

			if ((buffer[index] & 0b10000000) == 0b00000000) // if high bit is 0, 1 byte
			{
				rune.value = buffer[index + 0];
				index += 1;
				return ConversionError.None;
			}

			if ((buffer[index] & 0b11100000) == 0b11000000) // if high 3 bits are 110, 2 bytes
			{
				if (index + 2 > capacity)
				{
					index += 1;
					return ConversionError.Overflow;
				}

				code = (buffer[index + 0] & 0b00011111);
				code = (code << 6) | (buffer[index + 1] & 0b00111111);
				if (code < (1 << 7) || NotTrailer(buffer[index + 1]))
				{
					index += 1;
					return ConversionError.Encoding;
				}

				rune.value = code;
				index += 2;
				return ConversionError.None;
			}

			if ((buffer[index] & 0b11110000) == 0b11100000) // if high 4 bits are 1110, 3 bytes
			{
				if (index + 3 > capacity)
				{
					index += 1;
					return ConversionError.Overflow;
				}

				code = (buffer[index + 0] & 0b00001111);
				code = (code << 6) | (buffer[index + 1] & 0b00111111);
				code = (code << 6) | (buffer[index + 2] & 0b00111111);
				if (code < (1 << 11) || !IsValidCodePoint(code) || NotTrailer(buffer[index + 1]) ||
				    NotTrailer(buffer[index + 2]))
				{
					index += 1;
					return ConversionError.Encoding;
				}

				rune.value = code;
				index += 3;
				return ConversionError.None;
			}

			if ((buffer[index] & 0b11111000) == 0b11110000) // if high 5 bits are 11110, 4 bytes
			{
				if (index + 4 > capacity)
				{
					index += 1;
					return ConversionError.Overflow;
				}

				code = (buffer[index + 0] & 0b00000111);
				code = (code << 6) | (buffer[index + 1] & 0b00111111);
				code = (code << 6) | (buffer[index + 2] & 0b00111111);
				code = (code << 6) | (buffer[index + 3] & 0b00111111);
				if (code < (1 << 16) || !IsValidCodePoint(code) || NotTrailer(buffer[index + 1]) ||
				    NotTrailer(buffer[index + 2]) || NotTrailer(buffer[index + 3]))
				{
					index += 1;
					return ConversionError.Encoding;
				}

				rune.value = code;
				index += 4;
				return ConversionError.None;
			}

			index += 1;
			return ConversionError.Encoding;
		}

		/// <summary>
		/// Returns true if a char is a Unicode leading surrogate.
		/// </summary>
		/// <param name="c">The char.</param>
		/// <returns>True if the char is a Unicode leading surrogate.</returns>
		static bool IsLeadingSurrogate(char c)
		{
			return c >= 0xD800 && c <= 0xDBFF;
		}

		/// <summary>
		/// Returns true if a char is a Unicode trailing surrogate.
		/// </summary>
		/// <param name="c">The char.</param>
		/// <returns>True if the char is a Unicode trailing surrogate.</returns>
		static bool IsTrailingSurrogate(char c)
		{
			return c >= 0xDC00 && c <= 0xDFFF;
		}

		/// <summary>
		/// Reads a UTF-16 encoded character from a buffer.
		/// </summary>
		/// <param name="rune">Outputs the character read. If the read fails, rune is not set.</param>
		/// <param name="buffer">The buffer of chars to read.</param>
		/// <param name="index">Reference to a char index into the buffer. If the read succeeds, index is incremented by the
		/// size in chars of the character read. If the read fails, index is not incremented.</param>
		/// <param name="capacity">The size in chars of the buffer. Used to check that the read is in bounds.</param>
		/// <returns><see cref="ConversionError.None"/> if the read succeeds. Otherwise, returns <see cref="ConversionError.Overflow"/>.</returns>
		public static ConversionError Utf16ToUcs(out Rune rune, SafePtr<char> buffer, ref int index, int capacity)
		{
			int code = 0;
			rune = ReplacementCharacter;
			if (index + 1 > capacity)
				return ConversionError.Overflow;
			if (!IsLeadingSurrogate(buffer[index]) || (index + 2 > capacity))
			{
				rune.value = buffer[index];
				index += 1;
				return ConversionError.None;
			}

			code = (buffer[index + 0] & 0x03FF);
			char next = buffer[index + 1];
			if (!IsTrailingSurrogate(next))
			{
				rune.value = buffer[index];
				index += 1;
				return ConversionError.None;
			}

			code = (code << 10) | (buffer[index + 1] & 0x03FF);
			code += 0x10000;
			rune.value = code;
			index += 2;
			return ConversionError.None;
		}

		/// <summary>
		/// Writes a rune to a buffer as a UTF-8 encoded character.
		/// </summary>
		/// <param name="rune">The rune to encode.</param>
		/// <param name="buffer">The buffer to write to.</param>
		/// <param name="index">Reference to a byte index into the buffer. If the write succeeds, index is incremented by the
		/// size in bytes of the character written. If the write fails, index is not incremented.</param>
		/// <param name="capacity">The size in bytes of the buffer. Used to check that the write is in bounds.</param>
		/// <returns><see cref="ConversionError.None"/> if the write succeeds. Otherwise, returns <see cref="ConversionError.CodePoint"/>, <see cref="ConversionError.Overflow"/>, or <see cref="ConversionError.Encoding"/>.</returns>
		public static ConversionError UcsToUtf8(SafePtr buffer, ref int index, int capacity, Rune rune)
		{
			if (!IsValidCodePoint(rune.value))
			{
				return ConversionError.CodePoint;
			}

			if (index + 1 > capacity)
			{
				return ConversionError.Overflow;
			}

			if (rune.value <= 0x7F)
			{
				buffer[index++] = (byte)rune.value;
				return ConversionError.None;
			}

			if (rune.value <= 0x7FF)
			{
				if (index + 2 > capacity)
				{
					return ConversionError.Overflow;
				}

				buffer[index++] = (byte)(0xC0 | (rune.value >> 6));
				buffer[index++] = (byte)(0x80 | ((rune.value >> 0) & 0x3F));
				return ConversionError.None;
			}

			if (rune.value <= 0xFFFF)
			{
				if (index + 3 > capacity)
				{
					return ConversionError.Overflow;
				}

				buffer[index++] = (byte)(0xE0 | (rune.value >> 12));
				buffer[index++] = (byte)(0x80 | ((rune.value >> 6) & 0x3F));
				buffer[index++] = (byte)(0x80 | ((rune.value >> 0) & 0x3F));
				return ConversionError.None;
			}

			if (rune.value <= 0x1FFFFF)
			{
				if (index + 4 > capacity)
				{
					return ConversionError.Overflow;
				}

				buffer[index++] = (byte)(0xF0 | (rune.value >> 18));
				buffer[index++] = (byte)(0x80 | ((rune.value >> 12) & 0x3F));
				buffer[index++] = (byte)(0x80 | ((rune.value >> 6) & 0x3F));
				buffer[index++] = (byte)(0x80 | ((rune.value >> 0) & 0x3F));
				return ConversionError.None;
			}

			return ConversionError.Encoding;
		}

		/// <summary>
		/// Writes a rune to a buffer as a UTF-16 encoded character.
		/// </summary>
		/// <param name="rune">The rune to encode.</param>
		/// <param name="buffer">The buffer of chars to write to.</param>
		/// <param name="index">Reference to a char index into the buffer. If the write succeeds, index is incremented by the
		/// size in chars of the character written. If the write fails, index is not incremented.</param>
		/// <param name="capacity">The size in chars of the buffer. Used to check that the write is in bounds.</param>
		/// <returns><see cref="ConversionError.None"/> if the write succeeds. Otherwise, returns <see cref="ConversionError.CodePoint"/>, <see cref="ConversionError.Overflow"/>, or <see cref="ConversionError.Encoding"/>.</returns>
		public static ConversionError UcsToUtf16(SafePtr<char> buffer, ref int index, int capacity, Rune rune)
		{
			if (!IsValidCodePoint(rune.value))
			{
				return ConversionError.CodePoint;
			}

			if (index + 1 > capacity)
			{
				return ConversionError.Overflow;
			}

			if (rune.value >= 0x10000)
			{
				if (index + 2 > capacity)
				{
					return ConversionError.Overflow;
				}

				int code = rune.value - 0x10000;
				if (code >= (1 << 20))
				{
					return ConversionError.Encoding;
				}

				buffer[index++] = (char)(0xD800 | (code >> 10));
				buffer[index++] = (char)(0xDC00 | (code & 0x3FF));
				return ConversionError.None;
			}

			buffer[index++] = (char)rune.value;
			return ConversionError.None;
		}

		/// <summary>
		/// Copies UTF-16 characters from one buffer to another buffer as UTF-8.
		/// </summary>
		/// <remarks>Assumes the source data is valid UTF-16.</remarks>
		/// <param name="utf16Buffer">The source buffer.</param>
		/// <param name="utf16Length">The number of chars to read from the source.</param>
		/// <param name="utf8Buffer">The destination buffer.</param>
		/// <param name="utf8Length">Outputs the number of bytes written to the destination.</param>
		/// <param name="utf8Capacity">The size in bytes of the destination buffer.</param>
		/// <returns><see cref="ConversionError.None"/> if the copy fully completes. Otherwise, returns <see cref="ConversionError.Overflow"/>.</returns>
		public static ConversionError Utf16ToUtf8(SafePtr<char> utf16Buffer, int utf16Length, SafePtr utf8Buffer,
			out int utf8Length, int utf8Capacity)
		{
			utf8Length = 0;
			for (var utf16Offset = 0; utf16Offset < utf16Length;)
			{
				Utf16ToUcs(out var ucs, utf16Buffer, ref utf16Offset, utf16Length);
				if (UcsToUtf8(utf8Buffer, ref utf8Length, utf8Capacity, ucs) == ConversionError.Overflow)
					return ConversionError.Overflow;
			}

			return ConversionError.None;
		}

		/// <summary>
		/// Copies UTF-8 characters from one buffer to another.
		/// </summary>
		/// <remarks>Assumes the source data is valid UTF-8.</remarks>
		/// <param name="srcBuffer">The source buffer.</param>
		/// <param name="srcLength">The number of bytes to read from the source.</param>
		/// <param name="destBuffer">The destination buffer.</param>
		/// <param name="destLength">Outputs the number of bytes written to the destination.</param>
		/// <param name="destCapacity">The size in bytes of the destination buffer.</param>
		/// <returns><see cref="ConversionError.None"/> if the copy fully completes. Otherwise, returns <see cref="ConversionError.Overflow"/>.</returns>
		public static ConversionError Utf8ToUtf8(SafePtr srcBuffer, int srcLength, SafePtr destBuffer, out int destLength,
			int destCapacity)
		{
			if (destCapacity >= srcLength)
			{
				MemoryExt.MemCopy(srcBuffer, destBuffer, srcLength);
				destLength = srcLength;
				return ConversionError.None;
			}

			// TODO even in this case, it's possible to MemCpy all but the last 3 bytes that fit, and then by looking at only
			// TODO the high bits of the last 3 bytes that fit, decide how many of the 3 to append. but that requires a
			// TODO little UNICODE presence of mind that nobody has today.
			destLength = 0;
			for (var srcOffset = 0; srcOffset < srcLength;)
			{
				Utf8ToUcs(out var ucs, srcBuffer, ref srcOffset, srcLength);
				if (UcsToUtf8(destBuffer, ref destLength, destCapacity, ucs) == ConversionError.Overflow)
					return ConversionError.Overflow;
			}

			return ConversionError.None;
		}

		/// <summary>
		/// Copies UTF-8 characters from one buffer to another as UTF-16.
		/// </summary>
		/// <remarks>Assumes the source data is valid UTF-8.</remarks>
		/// <param name="utf8Buffer">The source buffer.</param>
		/// <param name="utf8Length">The number of bytes to read from the source.</param>
		/// <param name="utf16Buffer">The destination buffer.</param>
		/// <param name="utf16Length">Outputs the number of chars written to the destination.</param>
		/// <param name="utf16Capacity">The size in chars of the destination buffer.</param>
		/// <returns><see cref="ConversionError.None"/> if the copy fully completes. Otherwise, <see cref="ConversionError.Overflow"/>.</returns>
		public static ConversionError Utf8ToUtf16(SafePtr utf8Buffer, int utf8Length, SafePtr<char> utf16Buffer,
			out int utf16Length, int utf16Capacity)
		{
			utf16Length = 0;
			for (var utf8Offset
				     = 0;
			     utf8Offset < utf8Length;)
			{
				Utf8ToUcs(out var ucs, utf8Buffer, ref utf8Offset, utf8Length);
				if (UcsToUtf16(utf16Buffer, ref utf16Length, utf16Capacity, ucs) == ConversionError.Overflow)
					return ConversionError.Overflow;
			}

			return ConversionError.None;
		}
	}
}
