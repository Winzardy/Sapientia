namespace Sapientia.Collections.FixedString
{
	public static unsafe partial class FixedStringExt
	{
		/// <summary>
		/// Append two characters to this IFixedString.  This is used as a helper for internal formatting.
		/// </summary>
		internal static FormatError Append<T>(ref this T fs, char a, char b)
			where T : struct, IFixedString
		{
			FormatError err = FormatError.None;
			err |= fs.Append((Unicode.Rune)a);
			err |= fs.Append((Unicode.Rune)b);
			if (err != FormatError.None)
				return FormatError.Overflow;
			return FormatError.None;
		}

		/// <summary>
		/// Append three characters to this IFixedString.  This is used as a helper for internal formatting.
		/// </summary>
		internal static FormatError Append<T>(ref this T fs, char a, char b, char c)
			where T : struct, IFixedString
		{
			FormatError err = FormatError.None;
			err |= fs.Append((Unicode.Rune)a);
			err |= fs.Append((Unicode.Rune)b);
			err |= fs.Append((Unicode.Rune)c);
			if (err != FormatError.None)
				return FormatError.Overflow;
			return FormatError.None;
		}

		/// <summary>
		/// Append 'I' 'n' 'f' 'i' 'n' 'i' 't' 'y' characters to this IFixedString.  This is used as a helper for internal formatting.
		/// </summary>
		internal static FormatError Append<T>(ref this T fs, char a, char b, char c, char d, char e, char f, char g,
			char h)
			where T : struct, IFixedString
		{
			FormatError err = FormatError.None;
			err |= fs.Append((Unicode.Rune)a);
			err |= fs.Append((Unicode.Rune)b);
			err |= fs.Append((Unicode.Rune)c);
			err |= fs.Append((Unicode.Rune)d);
			err |= fs.Append((Unicode.Rune)e);
			err |= fs.Append((Unicode.Rune)f);
			err |= fs.Append((Unicode.Rune)g);
			err |= fs.Append((Unicode.Rune)h);
			if (err != FormatError.None)
				return FormatError.Overflow;
			return FormatError.None;
		}

		internal static FormatError AppendScientific<T>(ref this T fs, char* source, int sourceLength,
			int decimalExponent, char decimalSeparator = '.')
			where T : struct, IFixedString
		{
			FormatError error;
			if ((error = fs.Append(source[0])) != FormatError.None)
				return error;
			if (sourceLength > 1)
			{
				if ((error = fs.Append(decimalSeparator)) != FormatError.None)
					return error;
				for (var i = 1; i < sourceLength; ++i)
				{
					if ((error = fs.Append(source[i])) != FormatError.None)
						return error;
				}
			}

			if ((error = fs.Append('E')) != FormatError.None)
				return error;
			if (decimalExponent < 0)
			{
				if ((error = fs.Append('-')) != FormatError.None)
					return error;
				decimalExponent *= -1;
				decimalExponent -= sourceLength - 1;
			}
			else
			{
				if ((error = fs.Append('+')) != FormatError.None)
					return error;
				decimalExponent += sourceLength - 1;
			}

			var ascii = stackalloc char[2];
			const int decimalDigits = 2;
			for (var i = 0; i < decimalDigits; ++i)
			{
				var decimalDigit = decimalExponent % 10;
				ascii[1 - i] = (char)('0' + decimalDigit);
				decimalExponent /= 10;
			}

			for (var i = 0; i < decimalDigits; ++i)
				if ((error = fs.Append(ascii[i])) != FormatError.None)
					return error;
			return FormatError.None;
		}

		/// <summary>
		/// Check if runes a, b, c are found at offset offset
		/// </summary>
		/// <param name="offset">The target offset</param>
		/// <param name="a">rune a</param>
		/// <param name="b">rune b</param>
		/// <param name="c">rune c</param>
		/// <returns></returns>
		internal static bool Found<T>(ref this T fs, ref int offset, char a, char b, char c)
			where T : struct, IFixedString
		{
			int old = offset;
			if ((fs.Read(ref offset).value | 32) == a
			    && (fs.Read(ref offset).value | 32) == b
			    && (fs.Read(ref offset).value | 32) == c)
				return true;
			offset = old;
			return false;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="offset"></param>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <param name="c"></param>
		/// <param name="d"></param>
		/// <param name="e"></param>
		/// <param name="f"></param>
		/// <param name="g"></param>
		/// <param name="h"></param>
		/// <returns></returns>
		internal static bool Found<T>(ref this T fs, ref int offset, char a, char b, char c, char d, char e, char f,
			char g, char h)
			where T : struct, IFixedString
		{
			int old = offset;
			if ((fs.Read(ref offset).value | 32) == a
			    && (fs.Read(ref offset).value | 32) == b
			    && (fs.Read(ref offset).value | 32) == c
			    && (fs.Read(ref offset).value | 32) == d
			    && (fs.Read(ref offset).value | 32) == e
			    && (fs.Read(ref offset).value | 32) == f
			    && (fs.Read(ref offset).value | 32) == g
			    && (fs.Read(ref offset).value | 32) == h)
				return true;
			offset = old;
			return false;
		}
	}
}
