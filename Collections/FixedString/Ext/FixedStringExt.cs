using System;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.Collections.FixedString
{
	/// <summary>
	/// Provides extension methods for FixedString*N*.
	/// </summary>
	public static unsafe partial class FixedStringExt
	{
		/// <summary>
		/// Returns the index of the first occurrence of a byte sequence in this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="bytes">A byte sequence to search for within this string.</param>
		/// <param name="bytesLen">The number of bytes in the byte sequence.</param>
		/// <returns>The index of the first occurrence of the byte sequence in this string. Returns -1 if no occurrence is found.</returns>
		public static int IndexOf<T>(ref this T fs, SafePtr bytes, int bytesLen)
			where T : struct, IFixedString
		{
			var dst = fs.GetSafePtr();
			var dstLen = fs.Length;
			for (var i = 0; i <= dstLen - bytesLen; ++i)
			{
				for (var j = 0; j < bytesLen; ++j)
					if (dst[i + j] != bytes[j])
						goto end_of_loop;
				return i;
				end_of_loop :
				{
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns the index of the first occurrence of a byte sequence within a subrange of this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="bytes">A byte sequence to search for within this string.</param>
		/// <param name="bytesLen">The number of bytes in the byte sequence.</param>
		/// <param name="startIndex">The first index in this string to consider as the first byte of the byte sequence.</param>
		/// <param name="distance">The last index in this string to consider as the first byte of the byte sequence.</param>
		/// <returns>The index of the first occurrence of the byte sequence in this string. Returns -1 if no occurrence is found.</returns>
		public static int IndexOf<T>(ref this T fs, SafePtr bytes, int bytesLen, int startIndex,
			int distance = int.MaxValue)
			where T : struct, IFixedString
		{
			var dst = fs.GetSafePtr();
			var dstLen = fs.Length;
			var searchRange = (distance - 1).Min(dstLen - bytesLen);
			for (var i = startIndex; i <= searchRange; ++i)
			{
				for (var j = 0; j < bytesLen; ++j)
					if (dst[i + j] != bytes[j])
						goto end_of_loop;
				return i;
				end_of_loop :
				{
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns the index of the first occurrence of a substring within this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <typeparam name="T2">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="other">A substring to search for within this string.</param>
		/// <returns>The index of the first occurrence of the second string within this string. Returns -1 if no occurrence is found.</returns>
		public static int IndexOf<T, T2>(ref this T fs, ref T2 other)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.IndexOf(other.GetSafePtr(), other.Length);
		}

		/// <summary>
		/// Returns the index of the first occurrence of a substring within a subrange of this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <typeparam name="T2">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="other">A substring to search for within this string.</param>
		/// <param name="startIndex">The first index in this string to consider as an occurrence of the second string.</param>
		/// <param name="distance">The last index in this string to consider as an occurrence of the second string.</param>
		/// <returns>The index of the first occurrence of the substring within this string. Returns -1 if no occurrence is found.</returns>
		public static int IndexOf<T, T2>(ref this T fs, ref T2 other, int startIndex, int distance = Int32.MaxValue)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.IndexOf(other.GetSafePtr(), other.Length, startIndex, distance);
		}

		/// <summary>
		/// Returns true if a given substring occurs within this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <typeparam name="T2">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="other">A substring to search for within this string.</param>
		/// <returns>True if the substring occurs within this string.</returns>
		public static bool Contains<T, T2>(ref this T fs, ref T2 other)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.IndexOf(ref other) != -1;
		}

		/// <summary>
		/// Returns the index of the last occurrence of a byte sequence within this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="bytes">A byte sequence to search for within this string.</param>
		/// <param name="bytesLen">The number of bytes in the byte sequence.</param>
		/// <returns>The index of the last occurrence of the byte sequence within this string. Returns -1 if no occurrence is found.</returns>
		public static int LastIndexOf<T>(ref this T fs, SafePtr bytes, int bytesLen)
			where T : struct, IFixedString
		{
			var dst = fs.GetSafePtr();
			var dstLen = fs.Length;
			for (var i = dstLen - bytesLen; i >= 0; --i)
			{
				for (var j = 0; j < bytesLen; ++j)
					if (dst[i + j] != bytes[j])
						goto end_of_loop;
				return i;
				end_of_loop :
				{
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns the index of the last occurrence of a byte sequence within a subrange of this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="bytes">A byte sequence to search for within this string.</param>
		/// <param name="bytesLen">The number of bytes in the byte sequence.</param>
		/// <param name="startIndex">The smallest index in this string to consider as the first byte of the byte sequence.</param>
		/// <param name="distance">The greatest index in this string to consider as the first byte of the byte sequence.</param>
		/// <returns>The index of the last occurrence of the byte sequence within this string. Returns -1 if no occurrences found.</returns>
		public static int LastIndexOf<T>(ref this T fs, SafePtr bytes, int bytesLen, int startIndex,
			int distance = int.MaxValue)
			where T : struct, IFixedString
		{
			var dst = fs.GetSafePtr();
			var dstLen = fs.Length;
			startIndex = Math.Min(dstLen - bytesLen, startIndex);
			var searchrange = Math.Max(0, startIndex - distance);
			for (var i = startIndex; i >= searchrange; --i)
			{
				for (var j = 0; j < bytesLen; ++j)
					if (dst[i + j] != bytes[j])
						goto end_of_loop;
				return i;
				end_of_loop :
				{
				}
			}

			return -1;
		}

		/// <summary>
		/// Returns the index of the last occurrence of a substring within this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <typeparam name="T2">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="other">A substring to search for in the this string.</param>
		/// <returns>The index of the last occurrence of the substring within this string. Returns -1 if no occurrence is found.</returns>
		public static int LastIndexOf<T, T2>(ref this T fs, ref T2 other)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.LastIndexOf(other.GetSafePtr(), other.Length);
		}

		/// <summary>
		/// Returns the index of the last occurrence of a substring within a subrange of this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <typeparam name="T2">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to search.</param>
		/// <param name="other">A substring to search for within this string.</param>
		/// <param name="startIndex">The greatest index in this string to consider as an occurrence of the substring.</param>
		/// <param name="distance">The smallest index in this string to consider as an occurrence of the substring.</param>
		/// <returns>the index of the last occurrence of the substring within the first string. Returns -1 if no occurrence is found.</returns>
		public static int LastIndexOf<T, T2>(ref this T fs, ref T2 other, int startIndex, int distance = Int32.MaxValue)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.LastIndexOf(other.GetSafePtr(), other.Length, startIndex, distance);
		}

		/// <summary>
		/// Returns the sort position of this string relative to a byte sequence.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to compare.</param>
		/// <param name="bytes">A byte sequence to compare.</param>
		/// <param name="bytesLen">The number of bytes in the byte sequence.</param>
		/// <returns>A number denoting the sort position of this string relative to the byte sequence:
		///
		/// 0 denotes that this string and byte sequence have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the byte sequence.<br/>
		/// +1 denotes that this string should be sorted to follow the byte sequence.<br/>
		/// </returns>
		public static int CompareTo<T>(ref this T fs, SafePtr bytes, int bytesLen)
			where T : struct, IFixedString
		{
			var a = fs.GetSafePtr();
			var aa = fs.Length;
			int chars = aa < bytesLen ? aa : bytesLen;
			for (var i = 0; i < chars; ++i)
			{
				if (a[i] < bytes[i])
					return -1;
				if (a[i] > bytes[i])
					return 1;
			}

			if (aa < bytesLen)
				return -1;
			if (aa > bytesLen)
				return 1;
			return 0;
		}

		/// <summary>
		/// Returns the sort position of this string relative to another.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <typeparam name="T2">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to compare.</param>
		/// <param name="other">Another string to compare.</param>
		/// <returns>A number denoting the relative sort position of the strings:
		///
		/// 0 denotes that the strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this first string should be sorted to follow the other.<br/>
		/// </returns>
		public static int CompareTo<T, T2>(ref this T fs, ref T2 other)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.CompareTo(other.GetSafePtr(), other.Length);
		}

		/// <summary>
		/// Returns true if this string and a byte sequence are equal (meaning they have the same length and content).
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to compare for equality.</param>
		/// <param name="bytes">A sequence of bytes to compare for equality.</param>
		/// <param name="bytesLen">The number of bytes in the byte sequence.</param>
		/// <returns>True if this string and the byte sequence have the same length and if this string's character bytes match the byte sequence.</returns>
		public static bool Equals<T>(ref this T fs, SafePtr bytes, int bytesLen)
			where T : struct, IFixedString
		{
			var a = fs.GetSafePtr();
			var aa = fs.Length;
			if (aa != bytesLen)
				return false;
			if (a == bytes)
				return true;
			return fs.CompareTo(bytes, bytesLen) == 0;
		}

		/// <summary>
		/// Returns true if this string is equal to another.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <typeparam name="T2">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to compare for equality.</param>
		/// <param name="other">Another string to compare for equality.</param>
		/// <returns>true if the two strings have the same length and matching content.</returns>
		public static bool Equals<T, T2>(ref this T fs, ref T2 other)
			where T : struct, IFixedString
			where T2 : struct, IFixedString
		{
			return fs.Equals(other.GetSafePtr(), other.Length);
		}

		/// <summary>
		/// Returns the Unicode.Rune at an index of this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to read.</param>
		/// <param name="index">A reference to an index in bytes (not characters).</param>
		/// <returns>The Unicode.Rune (character) which starts at the byte index. Returns Unicode.BadRune
		/// if the byte(s) at the index do not form a valid UTF-8 encoded character.</returns>
		public static Unicode.Rune Peek<T>(ref this T fs, int index)
			where T : struct, IFixedString
		{
			if (index >= fs.Length)
				return Unicode.BadRune;
			Unicode.Utf8ToUcs(out var rune, fs.GetSafePtr(), ref index, fs.Capacity);
			return rune;
		}

		/// <summary>
		/// Returns the Unicode.Rune at an index of this string. Increments the index to the position of the next character.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to read.</param>
		/// <param name="index">A reference to an index in bytes (not characters). Incremented by 1 to 4 depending upon the UTF-8 encoded size of the character read.</param>
		/// <returns>The character (as a `Unicode.Rune`) which starts at the byte index. Returns `Unicode.BadRune`
		/// if the byte(s) at the index do not form a valid UTF-8 encoded character.</returns>
		public static Unicode.Rune Read<T>(ref this T fs, ref int index)
			where T : struct, IFixedString
		{
			if (index >= fs.Length)
				return Unicode.BadRune;
			Unicode.Utf8ToUcs(out var rune, fs.GetSafePtr(), ref index, fs.Capacity);
			return rune;
		}

		/// <summary>
		/// Writes a Unicode.Rune at an index of this string. Increments the index to the position of the next character.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to modify.</param>
		/// <param name="index">A reference to an index in bytes (not characters). Incremented by 1 to 4 depending upon the UTF-8 encoded size of the character written.</param>
		/// <param name="rune">A rune to write to the string, encoded as UTF-8.</param>
		/// <returns>FormatError.None if successful. Returns FormatError.Overflow if the index is invalid or if there is not enough space to store the encoded rune.</returns>
		public static FormatError Write<T>(ref this T fs, ref int index, Unicode.Rune rune)
			where T : struct, IFixedString
		{
			var err = Unicode.UcsToUtf8(fs.GetSafePtr(), ref index, fs.Capacity, rune);
			if (err != ConversionError.None)
				return FormatError.Overflow;
			return FormatError.None;
		}

		/// <summary>
		/// Returns a copy of this string as a managed string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to copy.</param>
		/// <returns>A copy of this string as a managed string.</returns>
#if BURST
		[NotBurstCompatible]
#endif
		public static String ConvertToString<T>(ref this T fs)
			where T : struct, IFixedString
		{
			var cRaw = stackalloc char[fs.Length * 2];
			var c = new SafePtr<char>(cRaw, fs.Length * 2);
			int length = 0;
			Unicode.Utf8ToUtf16(fs.GetSafePtr(), fs.Length, c, out length, fs.Length * 2);
			return new String(c.ptr, 0, length);
		}

		/// <summary>
		/// Returns a hash code of this string.
		/// </summary>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to get a hash code of.</param>
		/// <returns>A hash code of this string.</returns>
		public static int ComputeHashCode<T>(ref this T fs)
			where T : struct, IFixedString
		{
			return (int)UnsafeExt.Hash(fs.GetSafePtr().ptr, fs.Length);
		}

		/// <summary>
		/// Returns the effective size in bytes of this string.
		/// </summary>
		/// <remarks>
		/// "Effective size" is `Length + 3`, the number of bytes you need to copy when serializing the string.
		/// (The plus 3 accounts for the null-terminator byte and the 2 bytes that store the Length).
		///
		/// Useful for checking whether this string will fit in the space of a smaller FixedString*N*.
		/// </remarks>
		/// <typeparam name="T">A FixedString*N* type.</typeparam>
		/// <param name="fs">A string to get the effective size of.</param>
		/// <returns>The effective size in bytes of this string.</returns>
		public static int EffectiveSizeOf<T>(ref this T fs)
			where T : struct, IFixedString
		{
			return sizeof(ushort) + fs.Length + 1;
		}
	}
}
