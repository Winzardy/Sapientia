using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Data;
using Sapientia.Extensions;

namespace Sapientia.Collections.FixedString
{
	// A temporary copy of a struct is made before it is displayed in a C# debugger.
	// However, only the first element of data members with names is copied at this time.
	// Therefore, it's important that all data visible in the debugger, has a name
	// and includes no 'fixed' array. This is why we name every byte in the following struct.

	/// <summary>
	/// <undoc /> [FixedBytes will be removed]
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	public struct FixedBytes16
	{
		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(0)] public byte byte0000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1)] public byte byte0001;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2)] public byte byte0002;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3)] public byte byte0003;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4)] public byte byte0004;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(5)] public byte byte0005;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(6)] public byte byte0006;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(7)] public byte byte0007;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(8)] public byte byte0008;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(9)] public byte byte0009;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(10)] public byte byte0010;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(11)] public byte byte0011;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(12)] public byte byte0012;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(13)] public byte byte0013;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(14)] public byte byte0014;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(15)] public byte byte0015;
	}

	// A temporary copy of a struct is made before it is displayed in a C# debugger.
	// However, only the first element of data members with names is copied at this time.
	// Therefore, it's important that all data visible in the debugger, has a name
	// and includes no 'fixed' array. This is why we name every byte in the following struct.

	/// <summary>
	/// For internal use only.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 30)]

	public struct FixedBytes30
	{
		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(0)] public FixedBytes16 offset0000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(16)] public byte byte0016;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(17)] public byte byte0017;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(18)] public byte byte0018;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(19)] public byte byte0019;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(20)] public byte byte0020;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(21)] public byte byte0021;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(22)] public byte byte0022;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(23)] public byte byte0023;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(24)] public byte byte0024;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(25)] public byte byte0025;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(26)] public byte byte0026;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(27)] public byte byte0027;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(28)] public byte byte0028;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(29)] public byte byte0029;
	}

	/// <summary>
	/// An unmanaged UTF-8 string whose content is stored directly in the 32-byte struct.
	/// </summary>
	/// <remarks>
	/// The binary layout of this string is guaranteed, for now and all time, to be a length (a little-endian two byte integer)
	/// followed by the bytes of the characters (with no padding). A zero byte always immediately follows the last character.
	/// Effectively, the number of bytes for storing characters is 3 less than 32 (two length bytes and one null byte).
	///
	/// This layout is identical to a <see cref="FixedList32Bytes{T}"/> of bytes, thus allowing reinterpretation between FixedString32Bytes and FixedList32Bytes.
	///
	/// By virtue of being an unmanaged, non-allocated struct with no pointers, this string is fully compatible with jobs and Burst compilation.
	/// Unlike managed string types, these strings can be put in any unmanaged ECS components, FixedList, or any other unmanaged structs.
	/// </remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 32)]

	public unsafe struct FixedString32Bytes
		: IFixedString
			, IComparable<String>
			, IEquatable<String>
	{
		internal const ushort utf8MaxLengthInBytes = 29;

#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal ushort utf8LengthInBytes;
#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal FixedBytes30 bytes;

		/// <summary>
		/// Returns the maximum number of UTF-8 bytes that can be stored in this string.
		/// </summary>
		/// <returns>
		/// The maximum number of UTF-8 bytes that can be stored in this string.
		/// </returns>
		public static int UTF8MaxLengthInBytes => utf8MaxLengthInBytes;

		/// <summary>
		/// For internal use only. Use <see cref="ToString"/> instead.
		/// </summary>
		/// <value>For internal use only. Use <see cref="ToString"/> instead.</value>
#if UNITY_5_3_OR_NEWER
		[Unity.Properties.CreateProperty]
#endif
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public string Value => ToString();

		/// <summary>
		/// Returns a pointer to the character bytes.
		/// </summary>
		/// <returns>A pointer to the character bytes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr()
		{
			return bytes.AsSafePtr();
		}

		/// <summary>
		/// The current length in bytes of this string's content.
		/// </summary>
		/// <remarks>
		/// The length value does not include the null-terminator byte.
		/// </remarks>
		/// <param name="value">The new length in bytes of the string's content.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the new length is out of bounds.</exception>
		/// <value>
		/// The current length in bytes of this string's content.
		/// </value>
		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8LengthInBytes;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckLengthInRange(value);
				utf8LengthInBytes = (ushort)value;
				GetSafePtr()[utf8LengthInBytes] = 0;
			}
		}

		/// <summary>
		/// The number of bytes this string has for storing UTF-8 characters.
		/// </summary>
		/// <value>The number of bytes this string has for storing UTF-8 characters.</value>
		/// <remarks>
		/// Does not include the null-terminator byte.
		///
		/// A setter is included for conformity with <see cref="INativeList{T}"/>, but <see cref="Capacity"/> is fixed at 29.
		/// Setting the value to anything other than 29 throws an exception.
		///
		/// In UTF-8 encoding, each Unicode code point (character) requires 1 to 4 bytes,
		/// so the number of characters that can be stored may be less than the capacity.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set the capacity to anything other than 29.</exception>
		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8MaxLengthInBytes;
		}

		/// <summary>
		/// Attempts to set the length in bytes. Does nothing if the new length is invalid.
		/// </summary>
		/// <param name="newLength">The desired length.</param>
		/// <param name="clearOptions">Whether added or removed bytes should be cleared (zeroed). (Increasing the length adds bytes; decreasing the length removes bytes.)</param>
		/// <returns>True if the new length is valid.</returns>
		public bool TryResize(int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength < 0 || newLength > utf8MaxLengthInBytes)
				return false;
			if (newLength == utf8LengthInBytes)
				return true;
			if (clearOptions == ClearOptions.ClearMemory)
			{
				if (newLength > utf8LengthInBytes)
					MemoryExt.MemClear(GetSafePtr() + utf8LengthInBytes, newLength - utf8LengthInBytes);
				else
					MemoryExt.MemClear(GetSafePtr() + newLength, utf8LengthInBytes - newLength);
			}

			utf8LengthInBytes = (ushort)newLength;
			// always null terminate
			GetSafePtr()[utf8LengthInBytes] = 0;

			return true;
		}

		/// <summary>
		/// Returns true if this string is empty (has no characters).
		/// </summary>
		/// <value>True if this string is empty (has no characters).</value>
		public bool IsEmpty => utf8LengthInBytes == 0;

		/// <summary>
		/// Returns the byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <value>The byte at the index.</value>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckIndexInRange(index);
				return GetSafePtr()[index];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckIndexInRange(index);
				GetSafePtr()[index] = value;
			}
		}

		/// <summary>
		/// Returns the reference to a byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <returns>A reference to the byte at the index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public ref byte ElementAt(int index)
		{
			CheckIndexInRange(index);
			return ref GetSafePtr()[index];
		}

		/// <summary>
		/// Sets the length to 0.
		/// </summary>
		public void Clear()
		{
			Length = 0;
		}

		/// <summary>
		/// Appends a byte.
		/// </summary>
		/// <remarks>
		/// A zero byte will always follow the newly appended byte.
		///
		/// No validation is performed: it is your responsibility for the bytes of the string to form valid UTF-8 when you're done appending bytes.
		/// </remarks>
		/// <param name="value">A byte to append.</param>
		public void Add(in byte value)
		{
			this[Length++] = value;
		}

		/// <summary>
		/// An enumerator over the characters (not bytes) of a FixedString32Bytes.
		/// </summary>
		/// <remarks>
		/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
		/// The first <see cref="MoveNext"/> call advances the enumerator's index to the first character.
		/// </remarks>
		public struct Enumerator : IEnumerator
		{
			private FixedString32Bytes _target;
			private int _offset;
			private Unicode.Rune _current;

			/// <summary>
			/// Initializes and returns an instance of FixedString32Bytes.Enumerator.
			/// </summary>
			/// <param name="other">A FixeString32 for which to create an enumerator.</param>
			public Enumerator(FixedString32Bytes other)
			{
				_target = other;
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// Advances the enumerator to the next character.
			/// </summary>
			/// <returns>True if <see cref="Current"/> is valid to read after the call.</returns>
			public bool MoveNext()
			{
				if (_offset >= _target.Length)
					return false;

				Unicode.Utf8ToUcs(out _current, _target.GetSafePtr(), ref _offset, _target.Length);

				return true;
			}

			/// <summary>
			/// Resets the enumerator to its initial state.
			/// </summary>
			public void Reset()
			{
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// The current character.
			/// </summary>
			/// <remarks>
			/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
			/// </remarks>
			/// <value>The current character.</value>
			public Unicode.Rune Current => _current;

			object IEnumerator.Current => Current;
		}

		/// <summary>
		/// Returns an enumerator for iterating over the characters of this string.
		/// </summary>
		/// <returns>An enumerator for iterating over the characters of the FixedString32Bytes.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A `System.String` to compare with.</param>
		/// <returns>An integer denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other string.<br/>
		/// +1 denotes that this string should be sorted to follow the other string.<br/>
		/// </returns>
		public int CompareTo(string other)
		{
			return ToString().CompareTo(other);
		}

		/// <summary>
		/// Returns true if this string and another have the same length and all the same characters.
		/// </summary>
		/// <param name="other">A string to compare for equality.</param>
		/// <returns>True if this string and the other have the same length and all the same characters.</returns>
		public bool Equals(string other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.Length;
			SafePtr aptr = bytes.AsSafePtr();
			fixed (char* bptrRaw = other)
			{
				var bptr = new SafePtr<Char>(bptrRaw, blen);
				return UTF8Ext.StrCmp(aptr, alen, bptr, blen) == 0;
			}
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString32Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		public FixedString32Bytes(string source)
		{
			this = default;
			var error = Initialize(source);
			CheckCopyError((CopyError)error, source);
		}

		/// <summary>
		/// Initializes an instance of FixedString32Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(string source)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			fixed (char* sourceptrRaw = source)
			{
				var sourceptr = new SafePtr<Char>(sourceptrRaw, source.Length);
				var error = UTF8Ext.Copy(GetSafePtr(), out utf8LengthInBytes, utf8MaxLengthInBytes,
					sourceptr, source.Length);
				if (error == CopyError.Truncation)
				{
#if UNITY_5_3_OR_NEWER
					UnityEngine.Debug.LogWarning($"Warning: {error} [string: \"{source}\"]");
#endif
				}
				else if (error != CopyError.None)
					return (int)error;
				this.Length = utf8LengthInBytes;
			}

			return 0;
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString32Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		public FixedString32Bytes(Unicode.Rune rune, int count = 1)
		{
			this = default;
			Initialize(rune, count);
		}

		/// <summary>
		/// Initializes an instance of FixedString32Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(Unicode.Rune rune, int count = 1)
		{
			this = default;
			return (int)this.Append(rune, count);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString32Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString32Bytes.</exception>
		public FixedString32Bytes(ref FixedString32Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString32Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString32Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString32Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString64Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString32Bytes.</exception>
		public FixedString32Bytes(ref FixedString64Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString64Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString64Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString64Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString128Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString32Bytes.</exception>
		public FixedString32Bytes(ref FixedString128Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString128Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString128Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString128Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString512Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString32Bytes.</exception>
		public FixedString32Bytes(ref FixedString512Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString512Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		public bool Equals(ref FixedString512Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString4096Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString32Bytes.</exception>
		public FixedString32Bytes(ref FixedString4096Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString4096Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString4096Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString4096Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns a new FixedString32Bytes that is a copy of another string.
		/// </summary>
		/// <param name="b">A string to copy.</param>
		/// <returns>A new FixedString32Bytes that is a copy of another string.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString32Bytes.</exception>
		public static implicit operator FixedString32Bytes(string b) => new FixedString32Bytes(b);

		/// <summary>
		/// Returns a new managed string that is a copy of this string.
		/// </summary>
		/// <returns>A new managed string that is a copy of this string.</returns>
		public override string ToString()
		{
			return this.ConvertToString();
		}

		/// <summary>
		/// Returns a hash code of this string.
		/// </summary>
		/// <remarks>Only the character bytes are included in the hash: any bytes beyond <see cref="Length"/> are not part of the hash.</remarks>
		/// <returns>The hash code of this string.</returns>
		public override int GetHashCode()
		{
			return this.ComputeHashCode();
		}

		/// <summary>
		/// Returns true if this string and an object are equal.
		/// </summary>
		/// <remarks>
		/// Returns false if the object is neither a System.String or a FixedString.
		///
		/// Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="obj">An object to compare for equality.</param>
		/// <returns>True if this string and the object are equal.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj is string aString) return Equals(aString);
			if (obj is FixedString32Bytes aFixedString32Bytes) return Equals(aFixedString32Bytes);
			if (obj is FixedString64Bytes aFixedString64Bytes) return Equals(aFixedString64Bytes);
			if (obj is FixedString128Bytes aFixedString128Bytes) return Equals(aFixedString128Bytes);
			if (obj is FixedString512Bytes aFixedString512Bytes) return Equals(aFixedString512Bytes);
			if (obj is FixedString4096Bytes aFixedString4096Bytes) return Equals(aFixedString4096Bytes);
			return false;
		}

		[Conditional("DEBUG")]
		private void CheckIndexInRange(int index)
		{
			if (index < 0)
				throw new IndexOutOfRangeException($"Index {index} must be positive.");
			if (index >= utf8LengthInBytes)
				throw new IndexOutOfRangeException(
					$"Index {index} is out of range in FixedString32Bytes of '{utf8LengthInBytes}' Length.");
		}

		[Conditional("DEBUG")]
		private void CheckLengthInRange(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException($"Length {length} must be positive.");
			if (length > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Length {length} is out of range in FixedString32Bytes of '{utf8MaxLengthInBytes}' Capacity.");
		}

		[Conditional("DEBUG")]
		private void CheckCapacityInRange(int capacity)
		{
			if (capacity > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Capacity {capacity} must be lower than {utf8MaxLengthInBytes}.");
		}

		[Conditional("DEBUG")]
		private static void CheckCopyError(CopyError error, string source)
		{
			if (error != CopyError.None)
				throw new ArgumentException($"FixedString32Bytes: {error} while copying \"{source}\"");
		}

		[Conditional("DEBUG")]
		private static void CheckFormatError(FormatError error)
		{
			if (error != FormatError.None)
				throw new ArgumentException("Source is too long to fit into fixed string of this size");
		}
	}

	// A temporary copy of a struct is made before it is displayed in a C# debugger.
	// However, only the first element of data members with names is copied at this time.
	// Therefore, it's important that all data visible in the debugger, has a name
	// and includes no 'fixed' array. This is why we name every byte in the following struct.

	/// <summary>
	/// For internal use only.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 62)]

	public struct FixedBytes62
	{
		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(0)] public FixedBytes16 offset0000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(16)] public FixedBytes16 offset0016;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(32)] public FixedBytes16 offset0032;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(48)] public byte byte0048;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(49)] public byte byte0049;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(50)] public byte byte0050;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(51)] public byte byte0051;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(52)] public byte byte0052;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(53)] public byte byte0053;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(54)] public byte byte0054;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(55)] public byte byte0055;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(56)] public byte byte0056;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(57)] public byte byte0057;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(58)] public byte byte0058;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(59)] public byte byte0059;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(60)] public byte byte0060;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(61)] public byte byte0061;
	}

	/// <summary>
	/// An unmanaged UTF-8 string whose content is stored directly in the 64-byte struct.
	/// </summary>
	/// <remarks>
	/// The binary layout of this string is guaranteed, for now and all time, to be a length (a little-endian two byte integer)
	/// followed by the bytes of the characters (with no padding). A zero byte always immediately follows the last character.
	/// Effectively, the number of bytes for storing characters is 3 less than 64 (two length bytes and one null byte).
	///
	/// This layout is identical to a <see cref="FixedList64Bytes{T}"/> of bytes, thus allowing reinterpretation between FixedString64Bytes and FixedList64Bytes.
	///
	/// By virtue of being an unmanaged, non-allocated struct with no pointers, this string is fully compatible with jobs and Burst compilation.
	/// Unlike managed string types, these strings can be put in any unmanaged ECS components, FixedList, or any other unmanaged structs.
	/// </remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 64)]

	public unsafe struct FixedString64Bytes
		: IFixedString
			, IComparable<String>
			, IEquatable<String>
	{
		internal const ushort utf8MaxLengthInBytes = 61;

#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal ushort utf8LengthInBytes;
#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal FixedBytes62 bytes;

		/// <summary>
		/// Returns the maximum number of UTF-8 bytes that can be stored in this string.
		/// </summary>
		/// <returns>
		/// The maximum number of UTF-8 bytes that can be stored in this string.
		/// </returns>
		public static int UTF8MaxLengthInBytes => utf8MaxLengthInBytes;

		/// <summary>
		/// For internal use only. Use <see cref="ToString"/> instead.
		/// </summary>
		/// <value>For internal use only. Use <see cref="ToString"/> instead.</value>
#if UNITY_5_3_OR_NEWER
		[Unity.Properties.CreateProperty]
#endif
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public string Value => ToString();

		/// <summary>
		/// Returns a pointer to the character bytes.
		/// </summary>
		/// <returns>A pointer to the character bytes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr()
		{
			return bytes.AsSafePtr();
		}

		/// <summary>
		/// The current length in bytes of this string's content.
		/// </summary>
		/// <remarks>
		/// The length value does not include the null-terminator byte.
		/// </remarks>
		/// <param name="value">The new length in bytes of the string's content.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the new length is out of bounds.</exception>
		/// <value>
		/// The current length in bytes of this string's content.
		/// </value>
		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8LengthInBytes;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckLengthInRange(value);
				utf8LengthInBytes = (ushort)value;
				GetSafePtr()[utf8LengthInBytes] = 0;
			}
		}

		/// <summary>
		/// The number of bytes this string has for storing UTF-8 characters.
		/// </summary>
		/// <value>The number of bytes this string has for storing UTF-8 characters.</value>
		/// <remarks>
		/// Does not include the null-terminator byte.
		///
		/// A setter is included for conformity with <see cref="INativeList{T}"/>, but <see cref="Capacity"/> is fixed at 61.
		/// Setting the value to anything other than 61 throws an exception.
		///
		/// In UTF-8 encoding, each Unicode code point (character) requires 1 to 4 bytes,
		/// so the number of characters that can be stored may be less than the capacity.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set the capacity to anything other than 61.</exception>
		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8MaxLengthInBytes;
		}

		/// <summary>
		/// Attempts to set the length in bytes. Does nothing if the new length is invalid.
		/// </summary>
		/// <param name="newLength">The desired length.</param>
		/// <param name="clearOptions">Whether added or removed bytes should be cleared (zeroed). (Increasing the length adds bytes; decreasing the length removes bytes.)</param>
		/// <returns>True if the new length is valid.</returns>
		public bool TryResize(int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength < 0 || newLength > utf8MaxLengthInBytes)
				return false;
			if (newLength == utf8LengthInBytes)
				return true;
			if (clearOptions == ClearOptions.ClearMemory)
			{
				if (newLength > utf8LengthInBytes)
					MemoryExt.MemClear(GetSafePtr() + utf8LengthInBytes, newLength - utf8LengthInBytes);
				else
					MemoryExt.MemClear(GetSafePtr() + newLength, utf8LengthInBytes - newLength);
			}

			utf8LengthInBytes = (ushort)newLength;
			// always null terminate
			GetSafePtr()[utf8LengthInBytes] = 0;

			return true;
		}

		/// <summary>
		/// Returns true if this string is empty (has no characters).
		/// </summary>
		/// <value>True if this string is empty (has no characters).</value>
		public bool IsEmpty => utf8LengthInBytes == 0;

		/// <summary>
		/// Returns the byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <value>The byte at the index.</value>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckIndexInRange(index);
				return GetSafePtr()[index];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckIndexInRange(index);
				GetSafePtr()[index] = value;
			}
		}

		/// <summary>
		/// Returns the reference to a byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <returns>A reference to the byte at the index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public ref byte ElementAt(int index)
		{
			CheckIndexInRange(index);
			return ref GetSafePtr()[index];
		}

		/// <summary>
		/// Sets the length to 0.
		/// </summary>
		public void Clear()
		{
			Length = 0;
		}

		/// <summary>
		/// Appends a byte.
		/// </summary>
		/// <remarks>
		/// A zero byte will always follow the newly appended byte.
		///
		/// No validation is performed: it is your responsibility for the bytes of the string to form valid UTF-8 when you're done appending bytes.
		/// </remarks>
		/// <param name="value">A byte to append.</param>
		public void Add(in byte value)
		{
			this[Length++] = value;
		}

		/// <summary>
		/// An enumerator over the characters (not bytes) of a FixedString64Bytes.
		/// </summary>
		/// <remarks>
		/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
		/// The first <see cref="MoveNext"/> call advances the enumerator's index to the first character.
		/// </remarks>
		public struct Enumerator : IEnumerator
		{
			private FixedString64Bytes _target;
			private int _offset;
			private Unicode.Rune _current;

			/// <summary>
			/// Initializes and returns an instance of FixedString64Bytes.Enumerator.
			/// </summary>
			/// <param name="other">A FixeString64 for which to create an enumerator.</param>
			public Enumerator(FixedString64Bytes other)
			{
				_target = other;
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// Advances the enumerator to the next character.
			/// </summary>
			/// <returns>True if <see cref="Current"/> is valid to read after the call.</returns>
			public bool MoveNext()
			{
				if (_offset >= _target.Length)
					return false;

				Unicode.Utf8ToUcs(out _current, _target.GetSafePtr(), ref _offset, _target.Length);

				return true;
			}

			/// <summary>
			/// Resets the enumerator to its initial state.
			/// </summary>
			public void Reset()
			{
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// The current character.
			/// </summary>
			/// <remarks>
			/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
			/// </remarks>
			/// <value>The current character.</value>
			public Unicode.Rune Current => _current;

			object IEnumerator.Current => Current;
		}

		/// <summary>
		/// Returns an enumerator for iterating over the characters of this string.
		/// </summary>
		/// <returns>An enumerator for iterating over the characters of the FixedString64Bytes.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A `System.String` to compare with.</param>
		/// <returns>An integer denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other string.<br/>
		/// +1 denotes that this string should be sorted to follow the other string.<br/>
		/// </returns>
		public int CompareTo(string other)
		{
			return ToString().CompareTo(other);
		}

		/// <summary>
		/// Returns true if this string and another have the same length and all the same characters.
		/// </summary>
		/// <param name="other">A string to compare for equality.</param>
		/// <returns>True if this string and the other have the same length and all the same characters.</returns>
		public bool Equals(string other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.Length;
			SafePtr aptr = bytes.AsSafePtr();
			fixed (char* bptrRaw = other)
			{
				var bptr = new SafePtr<Char>(bptrRaw, blen);
				return UTF8Ext.StrCmp(aptr, alen, bptr, blen) == 0;
			}
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString64Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		public FixedString64Bytes(string source)
		{
			this = default;
			var error = Initialize(source);
			CheckCopyError((CopyError)error, source);
		}

		/// <summary>
		/// Initializes an instance of FixedString64Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(string source)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			fixed (char* sourceptrRaw = source)
			{
				var sourceptr = new SafePtr<Char>(sourceptrRaw, source.Length);
				var error = UTF8Ext.Copy(GetSafePtr(), out utf8LengthInBytes, utf8MaxLengthInBytes,
					sourceptr, source.Length);
				if (error == CopyError.Truncation)
				{
#if UNITY_5_3_OR_NEWER
					UnityEngine.Debug.LogWarning($"Warning: {error} [string: \"{source}\"]");
#endif
				}
				else if (error != CopyError.None)
					return (int)error;
				this.Length = utf8LengthInBytes;
			}

			return 0;
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString64Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		public FixedString64Bytes(Unicode.Rune rune, int count = 1)
		{
			this = default;
			Initialize(rune, count);
		}

		/// <summary>
		/// Initializes an instance of FixedString64Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(Unicode.Rune rune, int count = 1)
		{
			this = default;
			return (int)this.Append(rune, count);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString32Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString64Bytes.</exception>
		public FixedString64Bytes(ref FixedString32Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString32Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString32Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString32Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString64Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString64Bytes.</exception>
		public FixedString64Bytes(ref FixedString64Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString64Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString64Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString64Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString128Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString64Bytes.</exception>
		public FixedString64Bytes(ref FixedString128Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString128Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString128Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString128Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString512Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString64Bytes.</exception>
		public FixedString64Bytes(ref FixedString512Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString512Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString512Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString512Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString4096Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString64Bytes.</exception>
		public FixedString64Bytes(ref FixedString4096Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString4096Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString4096Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString4096Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns a new FixedString64Bytes that is a copy of another string.
		/// </summary>
		/// <param name="b">A string to copy.</param>
		/// <returns>A new FixedString64Bytes that is a copy of another string.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString64Bytes.</exception>
		public static implicit operator FixedString64Bytes(string b) => new FixedString64Bytes(b);

		/// <summary>
		/// Returns a new managed string that is a copy of this string.
		/// </summary>
		/// <returns>A new managed string that is a copy of this string.</returns>
		public override string ToString()
		{
			return this.ConvertToString();
		}

		/// <summary>
		/// Returns a hash code of this string.
		/// </summary>
		/// <remarks>Only the character bytes are included in the hash: any bytes beyond <see cref="Length"/> are not part of the hash.</remarks>
		/// <returns>The hash code of this string.</returns>
		public override int GetHashCode()
		{
			return this.ComputeHashCode();
		}

		/// <summary>
		/// Returns true if this string and an object are equal.
		/// </summary>
		/// <remarks>
		/// Returns false if the object is neither a System.String or a FixedString.
		///
		/// Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="obj">An object to compare for equality.</param>
		/// <returns>True if this string and the object are equal.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj is string aString) return Equals(aString);
			if (obj is FixedString32Bytes aFixedString32Bytes) return Equals(aFixedString32Bytes);
			if (obj is FixedString64Bytes aFixedString64Bytes) return Equals(aFixedString64Bytes);
			if (obj is FixedString128Bytes aFixedString128Bytes) return Equals(aFixedString128Bytes);
			if (obj is FixedString512Bytes aFixedString512Bytes) return Equals(aFixedString512Bytes);
			if (obj is FixedString4096Bytes aFixedString4096Bytes) return Equals(aFixedString4096Bytes);
			return false;
		}

		[Conditional("DEBUG")]
		private void CheckIndexInRange(int index)
		{
			if (index < 0)
				throw new IndexOutOfRangeException($"Index {index} must be positive.");
			if (index >= utf8LengthInBytes)
				throw new IndexOutOfRangeException(
					$"Index {index} is out of range in FixedString64Bytes of '{utf8LengthInBytes}' Length.");
		}

		[Conditional("DEBUG")]
		private void CheckLengthInRange(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException($"Length {length} must be positive.");
			if (length > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Length {length} is out of range in FixedString64Bytes of '{utf8MaxLengthInBytes}' Capacity.");
		}

		[Conditional("DEBUG")]
		private void CheckCapacityInRange(int capacity)
		{
			if (capacity > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Capacity {capacity} must be lower than {utf8MaxLengthInBytes}.");
		}

		[Conditional("DEBUG")]
		private static void CheckCopyError(CopyError error, string source)
		{
			if (error != CopyError.None)
				throw new ArgumentException($"FixedString64Bytes: {error} while copying \"{source}\"");
		}

		[Conditional("DEBUG")]
		private static void CheckFormatError(FormatError error)
		{
			if (error != FormatError.None)
				throw new ArgumentException("Source is too long to fit into fixed string of this size");
		}
	}

	// A temporary copy of a struct is made before it is displayed in a C# debugger.
	// However, only the first element of data members with names is copied at this time.
	// Therefore, it's important that all data visible in the debugger, has a name
	// and includes no 'fixed' array. This is why we name every byte in the following struct.

	/// <summary>
	/// For internal use only.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 126)]

	public struct FixedBytes126
	{
		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(0)] public FixedBytes16 offset0000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(16)] public FixedBytes16 offset0016;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(32)] public FixedBytes16 offset0032;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(48)] public FixedBytes16 offset0048;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(64)] public FixedBytes16 offset0064;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(80)] public FixedBytes16 offset0080;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(96)] public FixedBytes16 offset0096;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(112)] public byte byte0112;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(113)] public byte byte0113;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(114)] public byte byte0114;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(115)] public byte byte0115;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(116)] public byte byte0116;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(117)] public byte byte0117;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(118)] public byte byte0118;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(119)] public byte byte0119;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(120)] public byte byte0120;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(121)] public byte byte0121;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(122)] public byte byte0122;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(123)] public byte byte0123;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(124)] public byte byte0124;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(125)] public byte byte0125;
	}

	/// <summary>
	/// An unmanaged UTF-8 string whose content is stored directly in the 128-byte struct.
	/// </summary>
	/// <remarks>
	/// The binary layout of this string is guaranteed, for now and all time, to be a length (a little-endian two byte integer)
	/// followed by the bytes of the characters (with no padding). A zero byte always immediately follows the last character.
	/// Effectively, the number of bytes for storing characters is 3 less than 128 (two length bytes and one null byte).
	///
	/// This layout is identical to a <see cref="FixedList128Bytes{T}"/> of bytes, thus allowing reinterpretation between FixedString128Bytes and FixedList128Bytes.
	///
	/// By virtue of being an unmanaged, non-allocated struct with no pointers, this string is fully compatible with jobs and Burst compilation.
	/// Unlike managed string types, these strings can be put in any unmanaged ECS components, FixedList, or any other unmanaged structs.
	/// </remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 128)]

	public unsafe struct FixedString128Bytes
		: IFixedString
			, IComparable<String>
			, IEquatable<String>
	{
		internal const ushort utf8MaxLengthInBytes = 125;

#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal ushort utf8LengthInBytes;
#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal FixedBytes126 bytes;

		/// <summary>
		/// Returns the maximum number of UTF-8 bytes that can be stored in this string.
		/// </summary>
		/// <returns>
		/// The maximum number of UTF-8 bytes that can be stored in this string.
		/// </returns>
		public static int UTF8MaxLengthInBytes => utf8MaxLengthInBytes;

		/// <summary>
		/// For internal use only. Use <see cref="ToString"/> instead.
		/// </summary>
		/// <value>For internal use only. Use <see cref="ToString"/> instead.</value>
#if UNITY_5_3_OR_NEWER
		[Unity.Properties.CreateProperty]
#endif
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public string Value => ToString();

		/// <summary>
		/// Returns a pointer to the character bytes.
		/// </summary>
		/// <returns>A pointer to the character bytes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr()
		{
			return bytes.AsSafePtr();
		}

		/// <summary>
		/// The current length in bytes of this string's content.
		/// </summary>
		/// <remarks>
		/// The length value does not include the null-terminator byte.
		/// </remarks>
		/// <param name="value">The new length in bytes of the string's content.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the new length is out of bounds.</exception>
		/// <value>
		/// The current length in bytes of this string's content.
		/// </value>
		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8LengthInBytes;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckLengthInRange(value);
				utf8LengthInBytes = (ushort)value;
				GetSafePtr()[utf8LengthInBytes] = 0;
			}
		}

		/// <summary>
		/// The number of bytes this string has for storing UTF-8 characters.
		/// </summary>
		/// <value>The number of bytes this string has for storing UTF-8 characters.</value>
		/// <remarks>
		/// Does not include the null-terminator byte.
		///
		/// A setter is included for conformity with <see cref="INativeList{T}"/>, but <see cref="Capacity"/> is fixed at 125.
		/// Setting the value to anything other than 125 throws an exception.
		///
		/// In UTF-8 encoding, each Unicode code point (character) requires 1 to 4 bytes,
		/// so the number of characters that can be stored may be less than the capacity.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set the capacity to anything other than 125.</exception>
		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8MaxLengthInBytes;
		}

		/// <summary>
		/// Attempts to set the length in bytes. Does nothing if the new length is invalid.
		/// </summary>
		/// <param name="newLength">The desired length.</param>
		/// <param name="clearOptions">Whether added or removed bytes should be cleared (zeroed). (Increasing the length adds bytes; decreasing the length removes bytes.)</param>
		/// <returns>True if the new length is valid.</returns>
		public bool TryResize(int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength < 0 || newLength > utf8MaxLengthInBytes)
				return false;
			if (newLength == utf8LengthInBytes)
				return true;
			if (clearOptions == ClearOptions.ClearMemory)
			{
				if (newLength > utf8LengthInBytes)
					MemoryExt.MemClear(GetSafePtr() + utf8LengthInBytes, newLength - utf8LengthInBytes);
				else
					MemoryExt.MemClear(GetSafePtr() + newLength, utf8LengthInBytes - newLength);
			}

			utf8LengthInBytes = (ushort)newLength;
			// always null terminate
			GetSafePtr()[utf8LengthInBytes] = 0;

			return true;
		}

		/// <summary>
		/// Returns true if this string is empty (has no characters).
		/// </summary>
		/// <value>True if this string is empty (has no characters).</value>
		public bool IsEmpty => utf8LengthInBytes == 0;

		/// <summary>
		/// Returns the byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <value>The byte at the index.</value>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckIndexInRange(index);
				return GetSafePtr()[index];
			}
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckIndexInRange(index);
				GetSafePtr()[index] = value;
			}
		}

		/// <summary>
		/// Returns the reference to a byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <returns>A reference to the byte at the index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public ref byte ElementAt(int index)
		{
			CheckIndexInRange(index);
			return ref GetSafePtr()[index];
		}

		/// <summary>
		/// Sets the length to 0.
		/// </summary>
		public void Clear()
		{
			Length = 0;
		}

		/// <summary>
		/// Appends a byte.
		/// </summary>
		/// <remarks>
		/// A zero byte will always follow the newly appended byte.
		///
		/// No validation is performed: it is your responsibility for the bytes of the string to form valid UTF-8 when you're done appending bytes.
		/// </remarks>
		/// <param name="value">A byte to append.</param>
		public void Add(in byte value)
		{
			this[Length++] = value;
		}

		/// <summary>
		/// An enumerator over the characters (not bytes) of a FixedString128Bytes.
		/// </summary>
		/// <remarks>
		/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
		/// The first <see cref="MoveNext"/> call advances the enumerator's index to the first character.
		/// </remarks>
		public struct Enumerator : IEnumerator
		{
			private FixedString128Bytes _target;
			private int _offset;
			private Unicode.Rune _current;

			/// <summary>
			/// Initializes and returns an instance of FixedString128Bytes.Enumerator.
			/// </summary>
			/// <param name="other">A FixeString128 for which to create an enumerator.</param>
			public Enumerator(FixedString128Bytes other)
			{
				_target = other;
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// Advances the enumerator to the next character.
			/// </summary>
			/// <returns>True if <see cref="Current"/> is valid to read after the call.</returns>
			public bool MoveNext()
			{
				if (_offset >= _target.Length)
					return false;

				Unicode.Utf8ToUcs(out _current, _target.GetSafePtr(), ref _offset, _target.Length);

				return true;
			}

			/// <summary>
			/// Resets the enumerator to its initial state.
			/// </summary>
			public void Reset()
			{
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// The current character.
			/// </summary>
			/// <remarks>
			/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
			/// </remarks>
			/// <value>The current character.</value>
			public Unicode.Rune Current => _current;

			object IEnumerator.Current => Current;
		}

		/// <summary>
		/// Returns an enumerator for iterating over the characters of this string.
		/// </summary>
		/// <returns>An enumerator for iterating over the characters of the FixedString128Bytes.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A `System.String` to compare with.</param>
		/// <returns>An integer denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other string.<br/>
		/// +1 denotes that this string should be sorted to follow the other string.<br/>
		/// </returns>
		public int CompareTo(string other)
		{
			return ToString().CompareTo(other);
		}

		/// <summary>
		/// Returns true if this string and another have the same length and all the same characters.
		/// </summary>
		/// <param name="other">A string to compare for equality.</param>
		/// <returns>True if this string and the other have the same length and all the same characters.</returns>
		public bool Equals(string other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.Length;
			SafePtr aptr = bytes.AsSafePtr();
			fixed (char* bptrRaw = other)
			{
				var bptr = new SafePtr<Char>(bptrRaw, blen);
				return UTF8Ext.StrCmp(aptr, alen, bptr, blen) == 0;
			}
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString128Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		public FixedString128Bytes(string source)
		{
			this = default;
			var error = Initialize(source);
			CheckCopyError((CopyError)error, source);
		}

		/// <summary>
		/// Initializes an instance of FixedString128Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(string source)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			fixed (char* sourceptrRaw = source)
			{
				var sourceptr = new SafePtr<Char>(sourceptrRaw, source.Length);
				var error = UTF8Ext.Copy(GetSafePtr(), out utf8LengthInBytes, utf8MaxLengthInBytes,
					sourceptr, source.Length);
				if (error == CopyError.Truncation)
				{
#if UNITY_5_3_OR_NEWER
					UnityEngine.Debug.LogWarning($"Warning: {error} [string: \"{source}\"]");
#endif
				}
				else if (error != CopyError.None)
					return (int)error;
				this.Length = utf8LengthInBytes;
			}

			return 0;
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString128Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		public FixedString128Bytes(Unicode.Rune rune, int count = 1)
		{
			this = default;
			Initialize(rune, count);
		}

		/// <summary>
		/// Initializes an instance of FixedString128Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(Unicode.Rune rune, int count = 1)
		{
			this = default;
			return (int)this.Append(rune, count);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString32Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString128Bytes.</exception>
		public FixedString128Bytes(ref FixedString32Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString32Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString32Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString32Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString64Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString128Bytes.</exception>
		public FixedString128Bytes(ref FixedString64Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString64Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString64Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString64Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString128Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString128Bytes.</exception>
		public FixedString128Bytes(ref FixedString128Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString128Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString128Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString128Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString512Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString128Bytes.</exception>
		public FixedString128Bytes(ref FixedString512Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString512Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString512Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString512Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString4096Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString128Bytes.</exception>
		public FixedString128Bytes(ref FixedString4096Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString4096Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString4096Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString4096Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns a new FixedString128Bytes that is a copy of another string.
		/// </summary>
		/// <param name="b">A string to copy.</param>
		/// <returns>A new FixedString128Bytes that is a copy of another string.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString128Bytes.</exception>
		public static implicit operator FixedString128Bytes(string b) => new FixedString128Bytes(b);

		/// <summary>
		/// Returns a new managed string that is a copy of this string.
		/// </summary>
		/// <returns>A new managed string that is a copy of this string.</returns>
		public override string ToString()
		{
			return this.ConvertToString();
		}

		/// <summary>
		/// Returns a hash code of this string.
		/// </summary>
		/// <remarks>Only the character bytes are included in the hash: any bytes beyond <see cref="Length"/> are not part of the hash.</remarks>
		/// <returns>The hash code of this string.</returns>
		public override int GetHashCode()
		{
			return this.ComputeHashCode();
		}

		/// <summary>
		/// Returns true if this string and an object are equal.
		/// </summary>
		/// <remarks>
		/// Returns false if the object is neither a System.String or a FixedString.
		///
		/// Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="obj">An object to compare for equality.</param>
		/// <returns>True if this string and the object are equal.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj is string aString) return Equals(aString);
			if (obj is FixedString32Bytes aFixedString32Bytes) return Equals(aFixedString32Bytes);
			if (obj is FixedString64Bytes aFixedString64Bytes) return Equals(aFixedString64Bytes);
			if (obj is FixedString128Bytes aFixedString128Bytes) return Equals(aFixedString128Bytes);
			if (obj is FixedString512Bytes aFixedString512Bytes) return Equals(aFixedString512Bytes);
			if (obj is FixedString4096Bytes aFixedString4096Bytes) return Equals(aFixedString4096Bytes);
			return false;
		}

		[Conditional("DEBUG")]
		private void CheckIndexInRange(int index)
		{
			if (index < 0)
				throw new IndexOutOfRangeException($"Index {index} must be positive.");
			if (index >= utf8LengthInBytes)
				throw new IndexOutOfRangeException(
					$"Index {index} is out of range in FixedString128Bytes of '{utf8LengthInBytes}' Length.");
		}

		[Conditional("DEBUG")]
		private void CheckLengthInRange(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException($"Length {length} must be positive.");
			if (length > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Length {length} is out of range in FixedString128Bytes of '{utf8MaxLengthInBytes}' Capacity.");
		}

		[Conditional("DEBUG")]
		private void CheckCapacityInRange(int capacity)
		{
			if (capacity > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Capacity {capacity} must be lower than {utf8MaxLengthInBytes}.");
		}

		[Conditional("DEBUG")]
		private static void CheckCopyError(CopyError error, string source)
		{
			if (error != CopyError.None)
				throw new ArgumentException($"FixedString128Bytes: {error} while copying \"{source}\"");
		}

		[Conditional("DEBUG")]
		private static void CheckFormatError(FormatError error)
		{
			if (error != FormatError.None)
				throw new ArgumentException("Source is too long to fit into fixed string of this size");
		}
	}

	// A temporary copy of a struct is made before it is displayed in a C# debugger.
	// However, only the first element of data members with names is copied at this time.
	// Therefore, it's important that all data visible in the debugger, has a name
	// and includes no 'fixed' array. This is why we name every byte in the following struct.

	/// <summary>
	/// For internal use only.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 510)]

	public struct FixedBytes510
	{
		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(0)] public FixedBytes16 offset0000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(16)] public FixedBytes16 offset0016;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(32)] public FixedBytes16 offset0032;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(48)] public FixedBytes16 offset0048;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(64)] public FixedBytes16 offset0064;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(80)] public FixedBytes16 offset0080;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(96)] public FixedBytes16 offset0096;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(112)] public FixedBytes16 offset0112;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(128)] public FixedBytes16 offset0128;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(144)] public FixedBytes16 offset0144;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(160)] public FixedBytes16 offset0160;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(176)] public FixedBytes16 offset0176;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(192)] public FixedBytes16 offset0192;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(208)] public FixedBytes16 offset0208;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(224)] public FixedBytes16 offset0224;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(240)] public FixedBytes16 offset0240;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(256)] public FixedBytes16 offset0256;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(272)] public FixedBytes16 offset0272;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(288)] public FixedBytes16 offset0288;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(304)] public FixedBytes16 offset0304;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(320)] public FixedBytes16 offset0320;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(336)] public FixedBytes16 offset0336;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(352)] public FixedBytes16 offset0352;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(368)] public FixedBytes16 offset0368;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(384)] public FixedBytes16 offset0384;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(400)] public FixedBytes16 offset0400;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(416)] public FixedBytes16 offset0416;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(432)] public FixedBytes16 offset0432;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(448)] public FixedBytes16 offset0448;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(464)] public FixedBytes16 offset0464;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(480)] public FixedBytes16 offset0480;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(496)] public byte byte0496;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(497)] public byte byte0497;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(498)] public byte byte0498;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(499)] public byte byte0499;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(500)] public byte byte0500;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(501)] public byte byte0501;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(502)] public byte byte0502;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(503)] public byte byte0503;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(504)] public byte byte0504;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(505)] public byte byte0505;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(506)] public byte byte0506;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(507)] public byte byte0507;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(508)] public byte byte0508;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(509)] public byte byte0509;
	}

	/// <summary>
	/// An unmanaged UTF-8 string whose content is stored directly in the 512-byte struct.
	/// </summary>
	/// <remarks>
	/// The binary layout of this string is guaranteed, for now and all time, to be a length (a little-endian two byte integer)
	/// followed by the bytes of the characters (with no padding). A zero byte always immediately follows the last character.
	/// Effectively, the number of bytes for storing characters is 3 less than 512 (two length bytes and one null byte).
	///
	/// This layout is identical to a <see cref="FixedList512Bytes{T}"/> of bytes, thus allowing reinterpretation between FixedString512Bytes and FixedList512Bytes.
	///
	/// By virtue of being an unmanaged, non-allocated struct with no pointers, this string is fully compatible with jobs and Burst compilation.
	/// Unlike managed string types, these strings can be put in any unmanaged ECS components, FixedList, or any other unmanaged structs.
	/// </remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 512)]

	public unsafe struct FixedString512Bytes
		: IFixedString
			, IComparable<String>
			, IEquatable<String>
	{
		internal const ushort utf8MaxLengthInBytes = 509;

#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal ushort utf8LengthInBytes;
#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal FixedBytes510 bytes;

		/// <summary>
		/// Returns the maximum number of UTF-8 bytes that can be stored in this string.
		/// </summary>
		/// <returns>
		/// The maximum number of UTF-8 bytes that can be stored in this string.
		/// </returns>
		public static int UTF8MaxLengthInBytes => utf8MaxLengthInBytes;

		/// <summary>
		/// For internal use only. Use <see cref="ToString"/> instead.
		/// </summary>
		/// <value>For internal use only. Use <see cref="ToString"/> instead.</value>
#if UNITY_5_3_OR_NEWER
		[Unity.Properties.CreateProperty]
#endif
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public string Value => ToString();

		/// <summary>
		/// Returns a pointer to the character bytes.
		/// </summary>
		/// <returns>A pointer to the character bytes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetSafePtr()
		{
			return bytes.AsSafePtr();
		}

		/// <summary>
		/// The current length in bytes of this string's content.
		/// </summary>
		/// <remarks>
		/// The length value does not include the null-terminator byte.
		/// </remarks>
		/// <param name="value">The new length in bytes of the string's content.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the new length is out of bounds.</exception>
		/// <value>
		/// The current length in bytes of this string's content.
		/// </value>
		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8LengthInBytes;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckLengthInRange(value);
				utf8LengthInBytes = (ushort)value;
				GetSafePtr()[utf8LengthInBytes] = 0;
			}
		}

		/// <summary>
		/// The number of bytes this string has for storing UTF-8 characters.
		/// </summary>
		/// <value>The number of bytes this string has for storing UTF-8 characters.</value>
		/// <remarks>
		/// Does not include the null-terminator byte.
		///
		/// A setter is included for conformity with <see cref="INativeList{T}"/>, but <see cref="Capacity"/> is fixed at 509.
		/// Setting the value to anything other than 509 throws an exception.
		///
		/// In UTF-8 encoding, each Unicode code point (character) requires 1 to 4 bytes,
		/// so the number of characters that can be stored may be less than the capacity.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set the capacity to anything other than 509.</exception>
		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8MaxLengthInBytes;
		}

		/// <summary>
		/// Attempts to set the length in bytes. Does nothing if the new length is invalid.
		/// </summary>
		/// <param name="newLength">The desired length.</param>
		/// <param name="clearOptions">Whether added or removed bytes should be cleared (zeroed). (Increasing the length adds bytes; decreasing the length removes bytes.)</param>
		/// <returns>True if the new length is valid.</returns>
		public bool TryResize(int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength < 0 || newLength > utf8MaxLengthInBytes)
				return false;
			if (newLength == utf8LengthInBytes)
				return true;
			if (clearOptions == ClearOptions.ClearMemory)
			{
				if (newLength > utf8LengthInBytes)
					MemoryExt.MemClear(GetSafePtr() + utf8LengthInBytes, newLength - utf8LengthInBytes);
				else
					MemoryExt.MemClear(GetSafePtr() + newLength, utf8LengthInBytes - newLength);
			}

			utf8LengthInBytes = (ushort)newLength;
			// always null terminate
			GetSafePtr()[utf8LengthInBytes] = 0;

			return true;
		}

		/// <summary>
		/// Returns true if this string is empty (has no characters).
		/// </summary>
		/// <value>True if this string is empty (has no characters).</value>
		public bool IsEmpty => utf8LengthInBytes == 0;

		/// <summary>
		/// Returns the byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <value>The byte at the index.</value>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckIndexInRange(index);
				return GetSafePtr()[index];
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckIndexInRange(index);
				GetSafePtr()[index] = value;
			}
		}

		/// <summary>
		/// Returns the reference to a byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <returns>A reference to the byte at the index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public ref byte ElementAt(int index)
		{
			CheckIndexInRange(index);
			return ref GetSafePtr()[index];
		}

		/// <summary>
		/// Sets the length to 0.
		/// </summary>
		public void Clear()
		{
			Length = 0;
		}

		/// <summary>
		/// Appends a byte.
		/// </summary>
		/// <remarks>
		/// A zero byte will always follow the newly appended byte.
		///
		/// No validation is performed: it is your responsibility for the bytes of the string to form valid UTF-8 when you're done appending bytes.
		/// </remarks>
		/// <param name="value">A byte to append.</param>
		public void Add(in byte value)
		{
			this[Length++] = value;
		}

		/// <summary>
		/// An enumerator over the characters (not bytes) of a FixedString512Bytes.
		/// </summary>
		/// <remarks>
		/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
		/// The first <see cref="MoveNext"/> call advances the enumerator's index to the first character.
		/// </remarks>
		public struct Enumerator : IEnumerator
		{
			private FixedString512Bytes _target;
			private int _offset;
			private Unicode.Rune _current;

			/// <summary>
			/// Initializes and returns an instance of FixedString512Bytes.Enumerator.
			/// </summary>
			/// <param name="other">A FixeString512 for which to create an enumerator.</param>
			public Enumerator(FixedString512Bytes other)
			{
				_target = other;
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// Advances the enumerator to the next character.
			/// </summary>
			/// <returns>True if <see cref="Current"/> is valid to read after the call.</returns>
			public bool MoveNext()
			{
				if (_offset >= _target.Length)
					return false;

				Unicode.Utf8ToUcs(out _current, _target.GetSafePtr(), ref _offset, _target.Length);

				return true;
			}

			/// <summary>
			/// Resets the enumerator to its initial state.
			/// </summary>
			public void Reset()
			{
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// The current character.
			/// </summary>
			/// <remarks>
			/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
			/// </remarks>
			/// <value>The current character.</value>
			public Unicode.Rune Current => _current;

			object IEnumerator.Current => Current;
		}

		/// <summary>
		/// Returns an enumerator for iterating over the characters of this string.
		/// </summary>
		/// <returns>An enumerator for iterating over the characters of the FixedString512Bytes.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A `System.String` to compare with.</param>
		/// <returns>An integer denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other string.<br/>
		/// +1 denotes that this string should be sorted to follow the other string.<br/>
		/// </returns>
		public int CompareTo(string other)
		{
			return ToString().CompareTo(other);
		}

		/// <summary>
		/// Returns true if this string and another have the same length and all the same characters.
		/// </summary>
		/// <param name="other">A string to compare for equality.</param>
		/// <returns>True if this string and the other have the same length and all the same characters.</returns>
		public bool Equals(string other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.Length;
			SafePtr aptr = bytes.AsSafePtr();
			fixed (char* bptrRaw = other)
			{
				var bptr = new SafePtr<Char>(bptrRaw, blen);
				return UTF8Ext.StrCmp(aptr, alen, bptr, blen) == 0;
			}
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString512Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		public FixedString512Bytes(string source)
		{
			this = default;
			var error = Initialize(source);
			CheckCopyError((CopyError)error, source);
		}

		/// <summary>
		/// Initializes an instance of FixedString512Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(string source)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			fixed (char* sourceptrRaw = source)
			{
				var sourceptr = new SafePtr<Char>(sourceptrRaw, source.Length);
				var error = UTF8Ext.Copy(GetSafePtr(), out utf8LengthInBytes, utf8MaxLengthInBytes,
					sourceptr, source.Length);
				if (error == CopyError.Truncation)
				{
#if UNITY_5_3_OR_NEWER
					UnityEngine.Debug.LogWarning($"Warning: {error} [string: \"{source}\"]");
#endif
				}
				else if (error != CopyError.None)
					return (int)error;
				this.Length = utf8LengthInBytes;
			}

			return 0;
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString512Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		public FixedString512Bytes(Unicode.Rune rune, int count = 1)
		{
			this = default;
			Initialize(rune, count);
		}

		/// <summary>
		/// Initializes an instance of FixedString512Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(Unicode.Rune rune, int count = 1)
		{
			this = default;
			return (int)this.Append(rune, count);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString32Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString512Bytes.</exception>
		public FixedString512Bytes(ref FixedString32Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString32Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString32Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString32Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString64Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString512Bytes.</exception>
		public FixedString512Bytes(ref FixedString64Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString64Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString64Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString64Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString128Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString512Bytes.</exception>
		public FixedString512Bytes(ref FixedString128Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString128Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString128Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString128Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString512Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString512Bytes.</exception>
		public FixedString512Bytes(ref FixedString512Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString512Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString512Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString512Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString4096Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString512Bytes.</exception>
		public FixedString512Bytes(ref FixedString4096Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString4096Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			int len = 0;
			SafePtr dstBytes = GetSafePtr();
			SafePtr srcBytes = other.bytes.AsSafePtr();
			var srcLength = other.utf8LengthInBytes;
			var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
				srcLength);
			if (error != FormatError.None)
				return (int)error;
			this.Length = len;

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString4096Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public bool Equals(ref FixedString4096Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns a new FixedString512Bytes that is a copy of another string.
		/// </summary>
		/// <param name="b">A string to copy.</param>
		/// <returns>A new FixedString512Bytes that is a copy of another string.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString512Bytes.</exception>
		public static implicit operator FixedString512Bytes(string b) => new FixedString512Bytes(b);

		/// <summary>
		/// Returns a new managed string that is a copy of this string.
		/// </summary>
		/// <returns>A new managed string that is a copy of this string.</returns>
		public override string ToString()
		{
			return this.ConvertToString();
		}

		/// <summary>
		/// Returns a hash code of this string.
		/// </summary>
		/// <remarks>Only the character bytes are included in the hash: any bytes beyond <see cref="Length"/> are not part of the hash.</remarks>
		/// <returns>The hash code of this string.</returns>
		public override int GetHashCode()
		{
			return this.ComputeHashCode();
		}

		/// <summary>
		/// Returns true if this string and an object are equal.
		/// </summary>
		/// <remarks>
		/// Returns false if the object is neither a System.String or a FixedString.
		///
		/// Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="obj">An object to compare for equality.</param>
		/// <returns>True if this string and the object are equal.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj is string aString) return Equals(aString);
			if (obj is FixedString32Bytes aFixedString32Bytes) return Equals(aFixedString32Bytes);
			if (obj is FixedString64Bytes aFixedString64Bytes) return Equals(aFixedString64Bytes);
			if (obj is FixedString128Bytes aFixedString128Bytes) return Equals(aFixedString128Bytes);
			if (obj is FixedString512Bytes aFixedString512Bytes) return Equals(aFixedString512Bytes);
			if (obj is FixedString4096Bytes aFixedString4096Bytes) return Equals(aFixedString4096Bytes);
			return false;
		}

		[Conditional("DEBUG")]
		private void CheckIndexInRange(int index)
		{
			if (index < 0)
				throw new IndexOutOfRangeException($"Index {index} must be positive.");
			if (index >= utf8LengthInBytes)
				throw new IndexOutOfRangeException(
					$"Index {index} is out of range in FixedString512Bytes of '{utf8LengthInBytes}' Length.");
		}

		[Conditional("DEBUG")]
		private void CheckLengthInRange(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException($"Length {length} must be positive.");
			if (length > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Length {length} is out of range in FixedString512Bytes of '{utf8MaxLengthInBytes}' Capacity.");
		}

		[Conditional("DEBUG")]
		private void CheckCapacityInRange(int capacity)
		{
			if (capacity > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Capacity {capacity} must be lower than {utf8MaxLengthInBytes}.");
		}

		[Conditional("DEBUG")]
		private static void CheckCopyError(CopyError error, string source)
		{
			if (error != CopyError.None)
				throw new ArgumentException($"FixedString512Bytes: {error} while copying \"{source}\"");
		}

		[Conditional("DEBUG")]
		private static void CheckFormatError(FormatError error)
		{
			if (error != FormatError.None)
				throw new ArgumentException("Source is too long to fit into fixed string of this size");
		}
	}

	// A temporary copy of a struct is made before it is displayed in a C# debugger.
	// However, only the first element of data members with names is copied at this time.
	// Therefore, it's important that all data visible in the debugger, has a name
	// and includes no 'fixed' array. This is why we name every byte in the following struct.

	/// <summary>
	/// For internal use only.
	/// </summary>
	[Serializable]
	[StructLayout(LayoutKind.Explicit, Size = 4094)]

	public struct FixedBytes4094
	{
		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(0)] public FixedBytes16 offset0000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(16)] public FixedBytes16 offset0016;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(32)] public FixedBytes16 offset0032;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(48)] public FixedBytes16 offset0048;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(64)] public FixedBytes16 offset0064;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(80)] public FixedBytes16 offset0080;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(96)] public FixedBytes16 offset0096;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(112)] public FixedBytes16 offset0112;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(128)] public FixedBytes16 offset0128;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(144)] public FixedBytes16 offset0144;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(160)] public FixedBytes16 offset0160;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(176)] public FixedBytes16 offset0176;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(192)] public FixedBytes16 offset0192;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(208)] public FixedBytes16 offset0208;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(224)] public FixedBytes16 offset0224;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(240)] public FixedBytes16 offset0240;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(256)] public FixedBytes16 offset0256;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(272)] public FixedBytes16 offset0272;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(288)] public FixedBytes16 offset0288;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(304)] public FixedBytes16 offset0304;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(320)] public FixedBytes16 offset0320;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(336)] public FixedBytes16 offset0336;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(352)] public FixedBytes16 offset0352;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(368)] public FixedBytes16 offset0368;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(384)] public FixedBytes16 offset0384;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(400)] public FixedBytes16 offset0400;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(416)] public FixedBytes16 offset0416;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(432)] public FixedBytes16 offset0432;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(448)] public FixedBytes16 offset0448;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(464)] public FixedBytes16 offset0464;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(480)] public FixedBytes16 offset0480;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(496)] public FixedBytes16 offset0496;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(512)] public FixedBytes16 offset0512;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(528)] public FixedBytes16 offset0528;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(544)] public FixedBytes16 offset0544;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(560)] public FixedBytes16 offset0560;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(576)] public FixedBytes16 offset0576;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(592)] public FixedBytes16 offset0592;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(608)] public FixedBytes16 offset0608;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(624)] public FixedBytes16 offset0624;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(640)] public FixedBytes16 offset0640;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(656)] public FixedBytes16 offset0656;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(672)] public FixedBytes16 offset0672;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(688)] public FixedBytes16 offset0688;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(704)] public FixedBytes16 offset0704;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(720)] public FixedBytes16 offset0720;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(736)] public FixedBytes16 offset0736;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(752)] public FixedBytes16 offset0752;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(768)] public FixedBytes16 offset0768;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(784)] public FixedBytes16 offset0784;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(800)] public FixedBytes16 offset0800;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(816)] public FixedBytes16 offset0816;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(832)] public FixedBytes16 offset0832;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(848)] public FixedBytes16 offset0848;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(864)] public FixedBytes16 offset0864;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(880)] public FixedBytes16 offset0880;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(896)] public FixedBytes16 offset0896;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(912)] public FixedBytes16 offset0912;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(928)] public FixedBytes16 offset0928;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(944)] public FixedBytes16 offset0944;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(960)] public FixedBytes16 offset0960;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(976)] public FixedBytes16 offset0976;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(992)] public FixedBytes16 offset0992;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1008)] public FixedBytes16 offset1008;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1024)] public FixedBytes16 offset1024;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1040)] public FixedBytes16 offset1040;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1056)] public FixedBytes16 offset1056;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1072)] public FixedBytes16 offset1072;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1088)] public FixedBytes16 offset1088;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1104)] public FixedBytes16 offset1104;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1120)] public FixedBytes16 offset1120;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1136)] public FixedBytes16 offset1136;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1152)] public FixedBytes16 offset1152;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1168)] public FixedBytes16 offset1168;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1184)] public FixedBytes16 offset1184;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1200)] public FixedBytes16 offset1200;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1216)] public FixedBytes16 offset1216;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1232)] public FixedBytes16 offset1232;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1248)] public FixedBytes16 offset1248;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1264)] public FixedBytes16 offset1264;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1280)] public FixedBytes16 offset1280;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1296)] public FixedBytes16 offset1296;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1312)] public FixedBytes16 offset1312;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1328)] public FixedBytes16 offset1328;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1344)] public FixedBytes16 offset1344;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1360)] public FixedBytes16 offset1360;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1376)] public FixedBytes16 offset1376;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1392)] public FixedBytes16 offset1392;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1408)] public FixedBytes16 offset1408;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1424)] public FixedBytes16 offset1424;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1440)] public FixedBytes16 offset1440;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1456)] public FixedBytes16 offset1456;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1472)] public FixedBytes16 offset1472;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1488)] public FixedBytes16 offset1488;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1504)] public FixedBytes16 offset1504;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1520)] public FixedBytes16 offset1520;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1536)] public FixedBytes16 offset1536;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1552)] public FixedBytes16 offset1552;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1568)] public FixedBytes16 offset1568;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1584)] public FixedBytes16 offset1584;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1600)] public FixedBytes16 offset1600;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1616)] public FixedBytes16 offset1616;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1632)] public FixedBytes16 offset1632;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1648)] public FixedBytes16 offset1648;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1664)] public FixedBytes16 offset1664;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1680)] public FixedBytes16 offset1680;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1696)] public FixedBytes16 offset1696;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1712)] public FixedBytes16 offset1712;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1728)] public FixedBytes16 offset1728;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1744)] public FixedBytes16 offset1744;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1760)] public FixedBytes16 offset1760;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1776)] public FixedBytes16 offset1776;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1792)] public FixedBytes16 offset1792;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1808)] public FixedBytes16 offset1808;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1824)] public FixedBytes16 offset1824;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1840)] public FixedBytes16 offset1840;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1856)] public FixedBytes16 offset1856;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1872)] public FixedBytes16 offset1872;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1888)] public FixedBytes16 offset1888;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1904)] public FixedBytes16 offset1904;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1920)] public FixedBytes16 offset1920;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1936)] public FixedBytes16 offset1936;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1952)] public FixedBytes16 offset1952;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1968)] public FixedBytes16 offset1968;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(1984)] public FixedBytes16 offset1984;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2000)] public FixedBytes16 offset2000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2016)] public FixedBytes16 offset2016;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2032)] public FixedBytes16 offset2032;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2048)] public FixedBytes16 offset2048;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2064)] public FixedBytes16 offset2064;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2080)] public FixedBytes16 offset2080;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2096)] public FixedBytes16 offset2096;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2112)] public FixedBytes16 offset2112;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2128)] public FixedBytes16 offset2128;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2144)] public FixedBytes16 offset2144;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2160)] public FixedBytes16 offset2160;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2176)] public FixedBytes16 offset2176;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2192)] public FixedBytes16 offset2192;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2208)] public FixedBytes16 offset2208;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2224)] public FixedBytes16 offset2224;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2240)] public FixedBytes16 offset2240;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2256)] public FixedBytes16 offset2256;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2272)] public FixedBytes16 offset2272;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2288)] public FixedBytes16 offset2288;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2304)] public FixedBytes16 offset2304;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2320)] public FixedBytes16 offset2320;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2336)] public FixedBytes16 offset2336;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2352)] public FixedBytes16 offset2352;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2368)] public FixedBytes16 offset2368;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2384)] public FixedBytes16 offset2384;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2400)] public FixedBytes16 offset2400;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2416)] public FixedBytes16 offset2416;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2432)] public FixedBytes16 offset2432;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2448)] public FixedBytes16 offset2448;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2464)] public FixedBytes16 offset2464;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2480)] public FixedBytes16 offset2480;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2496)] public FixedBytes16 offset2496;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2512)] public FixedBytes16 offset2512;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2528)] public FixedBytes16 offset2528;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2544)] public FixedBytes16 offset2544;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2560)] public FixedBytes16 offset2560;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2576)] public FixedBytes16 offset2576;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2592)] public FixedBytes16 offset2592;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2608)] public FixedBytes16 offset2608;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2624)] public FixedBytes16 offset2624;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2640)] public FixedBytes16 offset2640;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2656)] public FixedBytes16 offset2656;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2672)] public FixedBytes16 offset2672;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2688)] public FixedBytes16 offset2688;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2704)] public FixedBytes16 offset2704;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2720)] public FixedBytes16 offset2720;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2736)] public FixedBytes16 offset2736;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2752)] public FixedBytes16 offset2752;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2768)] public FixedBytes16 offset2768;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2784)] public FixedBytes16 offset2784;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2800)] public FixedBytes16 offset2800;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2816)] public FixedBytes16 offset2816;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2832)] public FixedBytes16 offset2832;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2848)] public FixedBytes16 offset2848;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2864)] public FixedBytes16 offset2864;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2880)] public FixedBytes16 offset2880;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2896)] public FixedBytes16 offset2896;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2912)] public FixedBytes16 offset2912;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2928)] public FixedBytes16 offset2928;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2944)] public FixedBytes16 offset2944;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2960)] public FixedBytes16 offset2960;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2976)] public FixedBytes16 offset2976;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(2992)] public FixedBytes16 offset2992;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3008)] public FixedBytes16 offset3008;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3024)] public FixedBytes16 offset3024;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3040)] public FixedBytes16 offset3040;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3056)] public FixedBytes16 offset3056;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3072)] public FixedBytes16 offset3072;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3088)] public FixedBytes16 offset3088;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3104)] public FixedBytes16 offset3104;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3120)] public FixedBytes16 offset3120;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3136)] public FixedBytes16 offset3136;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3152)] public FixedBytes16 offset3152;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3168)] public FixedBytes16 offset3168;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3184)] public FixedBytes16 offset3184;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3200)] public FixedBytes16 offset3200;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3216)] public FixedBytes16 offset3216;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3232)] public FixedBytes16 offset3232;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3248)] public FixedBytes16 offset3248;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3264)] public FixedBytes16 offset3264;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3280)] public FixedBytes16 offset3280;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3296)] public FixedBytes16 offset3296;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3312)] public FixedBytes16 offset3312;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3328)] public FixedBytes16 offset3328;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3344)] public FixedBytes16 offset3344;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3360)] public FixedBytes16 offset3360;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3376)] public FixedBytes16 offset3376;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3392)] public FixedBytes16 offset3392;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3408)] public FixedBytes16 offset3408;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3424)] public FixedBytes16 offset3424;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3440)] public FixedBytes16 offset3440;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3456)] public FixedBytes16 offset3456;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3472)] public FixedBytes16 offset3472;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3488)] public FixedBytes16 offset3488;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3504)] public FixedBytes16 offset3504;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3520)] public FixedBytes16 offset3520;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3536)] public FixedBytes16 offset3536;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3552)] public FixedBytes16 offset3552;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3568)] public FixedBytes16 offset3568;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3584)] public FixedBytes16 offset3584;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3600)] public FixedBytes16 offset3600;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3616)] public FixedBytes16 offset3616;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3632)] public FixedBytes16 offset3632;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3648)] public FixedBytes16 offset3648;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3664)] public FixedBytes16 offset3664;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3680)] public FixedBytes16 offset3680;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3696)] public FixedBytes16 offset3696;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3712)] public FixedBytes16 offset3712;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3728)] public FixedBytes16 offset3728;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3744)] public FixedBytes16 offset3744;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3760)] public FixedBytes16 offset3760;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3776)] public FixedBytes16 offset3776;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3792)] public FixedBytes16 offset3792;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3808)] public FixedBytes16 offset3808;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3824)] public FixedBytes16 offset3824;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3840)] public FixedBytes16 offset3840;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3856)] public FixedBytes16 offset3856;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3872)] public FixedBytes16 offset3872;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3888)] public FixedBytes16 offset3888;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3904)] public FixedBytes16 offset3904;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3920)] public FixedBytes16 offset3920;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3936)] public FixedBytes16 offset3936;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3952)] public FixedBytes16 offset3952;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3968)] public FixedBytes16 offset3968;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(3984)] public FixedBytes16 offset3984;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4000)] public FixedBytes16 offset4000;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4016)] public FixedBytes16 offset4016;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4032)] public FixedBytes16 offset4032;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4048)] public FixedBytes16 offset4048;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4064)] public FixedBytes16 offset4064;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4080)] public byte byte4080;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4081)] public byte byte4081;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4082)] public byte byte4082;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4083)] public byte byte4083;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4084)] public byte byte4084;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4085)] public byte byte4085;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4086)] public byte byte4086;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4087)] public byte byte4087;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4088)] public byte byte4088;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4089)] public byte byte4089;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4090)] public byte byte4090;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4091)] public byte byte4091;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4092)] public byte byte4092;

		/// <summary>
		/// For internal use only.
		/// </summary>
		[FieldOffset(4093)] public byte byte4093;
	}

	/// <summary>
	/// An unmanaged UTF-8 string whose content is stored directly in the 4096-byte struct.
	/// </summary>
	/// <remarks>
	/// The binary layout of this string is guaranteed, for now and all time, to be a length (a little-endian two byte integer)
	/// followed by the bytes of the characters (with no padding). A zero byte always immediately follows the last character.
	/// Effectively, the number of bytes for storing characters is 3 less than 4096 (two length bytes and one null byte).
	///
	/// This layout is identical to a <see cref="FixedList4096Bytes{T}"/> of bytes, thus allowing reinterpretation between FixedString4096Bytes and FixedList4096Bytes.
	///
	/// By virtue of being an unmanaged, non-allocated struct with no pointers, this string is fully compatible with jobs and Burst compilation.
	/// Unlike managed string types, these strings can be put in any unmanaged ECS components, FixedList, or any other unmanaged structs.
	/// </remarks>
	[Serializable]
	[StructLayout(LayoutKind.Sequential, Size = 4096)]

	public partial struct FixedString4096Bytes
		: IFixedString
			, IComparable<String>
			, IEquatable<String>
	{
		internal const ushort utf8MaxLengthInBytes = 4093;

#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal ushort utf8LengthInBytes;
#if UNITY_5_3_OR_NEWER
		[UnityEngine.SerializeField]
#endif
		internal FixedBytes4094 bytes;

		/// <summary>
		/// Returns the maximum number of UTF-8 bytes that can be stored in this string.
		/// </summary>
		/// <returns>
		/// The maximum number of UTF-8 bytes that can be stored in this string.
		/// </returns>
		public static int UTF8MaxLengthInBytes => utf8MaxLengthInBytes;

		/// <summary>
		/// For internal use only. Use <see cref="ToString"/> instead.
		/// </summary>
		/// <value>For internal use only. Use <see cref="ToString"/> instead.</value>
#if UNITY_5_3_OR_NEWER
		[Unity.Properties.CreateProperty]
#endif
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public string Value => ToString();

		/// <summary>
		/// Returns a pointer to the character bytes.
		/// </summary>
		/// <returns>A pointer to the character bytes.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public unsafe SafePtr GetSafePtr()
		{
			return bytes.AsSafePtr();
		}

		/// <summary>
		/// The current length in bytes of this string's content.
		/// </summary>
		/// <remarks>
		/// The length value does not include the null-terminator byte.
		/// </remarks>
		/// <param name="value">The new length in bytes of the string's content.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if the new length is out of bounds.</exception>
		/// <value>
		/// The current length in bytes of this string's content.
		/// </value>
		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8LengthInBytes;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				CheckLengthInRange(value);
				utf8LengthInBytes = (ushort)value;
				unsafe
				{
					GetSafePtr()[utf8LengthInBytes] = 0;
				}
			}
		}

		/// <summary>
		/// The number of bytes this string has for storing UTF-8 characters.
		/// </summary>
		/// <value>The number of bytes this string has for storing UTF-8 characters.</value>
		/// <remarks>
		/// Does not include the null-terminator byte.
		///
		/// A setter is included for conformity with <see cref="INativeList{T}"/>, but <see cref="Capacity"/> is fixed at 4093.
		/// Setting the value to anything other than 4093 throws an exception.
		///
		/// In UTF-8 encoding, each Unicode code point (character) requires 1 to 4 bytes,
		/// so the number of characters that can be stored may be less than the capacity.
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if attempting to set the capacity to anything other than 4093.</exception>
		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => utf8MaxLengthInBytes;
		}

		/// <summary>
		/// Attempts to set the length in bytes. Does nothing if the new length is invalid.
		/// </summary>
		/// <param name="newLength">The desired length.</param>
		/// <param name="clearOptions">Whether added or removed bytes should be cleared (zeroed). (Increasing the length adds bytes; decreasing the length removes bytes.)</param>
		/// <returns>True if the new length is valid.</returns>
		public bool TryResize(int newLength, ClearOptions clearOptions = ClearOptions.ClearMemory)
		{
			if (newLength < 0 || newLength > utf8MaxLengthInBytes)
				return false;
			if (newLength == utf8LengthInBytes)
				return true;
			unsafe
			{
				if (clearOptions == ClearOptions.ClearMemory)
				{
					if (newLength > utf8LengthInBytes)
						MemoryExt.MemClear(GetSafePtr() + utf8LengthInBytes, newLength - utf8LengthInBytes);
					else
						MemoryExt.MemClear(GetSafePtr() + newLength, utf8LengthInBytes - newLength);
				}

				utf8LengthInBytes = (ushort)newLength;
				// always null terminate
				GetSafePtr()[utf8LengthInBytes] = 0;
			}

			return true;
		}

		/// <summary>
		/// Returns true if this string is empty (has no characters).
		/// </summary>
		/// <value>True if this string is empty (has no characters).</value>
		public bool IsEmpty => utf8LengthInBytes == 0;

		/// <summary>
		/// Returns the byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <value>The byte at the index.</value>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				unsafe
				{
					CheckIndexInRange(index);
					return GetSafePtr()[index];
				}
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set
			{
				unsafe
				{
					CheckIndexInRange(index);
					GetSafePtr()[index] = value;
				}
			}
		}

		/// <summary>
		/// Returns the reference to a byte (not character) at an index.
		/// </summary>
		/// <param name="index">A byte index.</param>
		/// <returns>A reference to the byte at the index.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the index is out of bounds.</exception>
		public ref byte ElementAt(int index)
		{
			unsafe
			{
				CheckIndexInRange(index);
				return ref GetSafePtr()[index];
			}
		}

		/// <summary>
		/// Sets the length to 0.
		/// </summary>
		public void Clear()
		{
			Length = 0;
		}

		/// <summary>
		/// Appends a byte.
		/// </summary>
		/// <remarks>
		/// A zero byte will always follow the newly appended byte.
		///
		/// No validation is performed: it is your responsibility for the bytes of the string to form valid UTF-8 when you're done appending bytes.
		/// </remarks>
		/// <param name="value">A byte to append.</param>
		public void Add(in byte value)
		{
			this[Length++] = value;
		}

		/// <summary>
		/// An enumerator over the characters (not bytes) of a FixedString4096Bytes.
		/// </summary>
		/// <remarks>
		/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
		/// The first <see cref="MoveNext"/> call advances the enumerator's index to the first character.
		/// </remarks>
		public struct Enumerator : IEnumerator
		{
			private FixedString4096Bytes _target;
			private int _offset;
			private Unicode.Rune _current;

			/// <summary>
			/// Initializes and returns an instance of FixedString4096Bytes.Enumerator.
			/// </summary>
			/// <param name="other">A FixeString4096 for which to create an enumerator.</param>
			public Enumerator(FixedString4096Bytes other)
			{
				_target = other;
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// Advances the enumerator to the next character.
			/// </summary>
			/// <returns>True if <see cref="Current"/> is valid to read after the call.</returns>
			public bool MoveNext()
			{
				if (_offset >= _target.Length)
					return false;

				unsafe
				{
					Unicode.Utf8ToUcs(out _current, _target.GetSafePtr(), ref _offset, _target.Length);
				}

				return true;
			}

			/// <summary>
			/// Resets the enumerator to its initial state.
			/// </summary>
			public void Reset()
			{
				_offset = 0;
				_current = default;
			}

			/// <summary>
			/// The current character.
			/// </summary>
			/// <remarks>
			/// In an enumerator's initial state, <see cref="Current"/> is not valid to read.
			/// </remarks>
			/// <value>The current character.</value>
			public Unicode.Rune Current => _current;

			object IEnumerator.Current => Current;
		}

		/// <summary>
		/// Returns an enumerator for iterating over the characters of this string.
		/// </summary>
		/// <returns>An enumerator for iterating over the characters of the FixedString4096Bytes.</returns>
		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A `System.String` to compare with.</param>
		/// <returns>An integer denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other string.<br/>
		/// +1 denotes that this string should be sorted to follow the other string.<br/>
		/// </returns>
		public int CompareTo(string other)
		{
			return ToString().CompareTo(other);
		}

		/// <summary>
		/// Returns true if this string and another have the same length and all the same characters.
		/// </summary>
		/// <param name="other">A string to compare for equality.</param>
		/// <returns>True if this string and the other have the same length and all the same characters.</returns>
		public bool Equals(string other)
		{
			unsafe
			{
				int alen = utf8LengthInBytes;
				int blen = other.Length;
				SafePtr aptr = bytes.AsSafePtr();
				fixed (char* bptrRaw = other)
				{
					var bptr = new SafePtr<char>(bptrRaw, blen);
					return UTF8Ext.StrCmp(aptr, alen, bptr, blen) == 0;
				}
			}
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString4096Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		public FixedString4096Bytes(string source)
		{
			this = default;
			var error = Initialize(source);
			CheckCopyError((CopyError)error, source);
		}

		/// <summary>
		/// Initializes an instance of FixedString4096Bytes with the characters copied from a string.
		/// </summary>
		/// <param name="source">The source string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(string source)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			unsafe
			{
				fixed (char* sourceptrRaw = source)
				{
					var sourceptr = new SafePtr<char>(sourceptrRaw, source.Length);
					var error = UTF8Ext.Copy(GetSafePtr(), out utf8LengthInBytes, utf8MaxLengthInBytes,
						sourceptr, source.Length);
					if (error == CopyError.Truncation)
					{
#if UNITY_5_3_OR_NEWER
						UnityEngine.Debug.LogWarning($"Warning: {error} [string: \"{source}\"]");
#endif
					}
					else if (error != CopyError.None)
						return (int)error;
					this.Length = utf8LengthInBytes;
				}
			}

			return 0;
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString4096Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		public FixedString4096Bytes(Unicode.Rune rune, int count = 1)
		{
			this = default;
			Initialize(rune, count);
		}

		/// <summary>
		/// Initializes an instance of FixedString4096Bytes with a single character repeatedly appended some number of times.
		/// </summary>
		/// <param name="rune">The Unicode.Rune to repeat.</param>
		/// <param name="count">The number of times to repeat the character. Default is 1.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(Unicode.Rune rune, int count = 1)
		{
			this = default;
			return (int)this.Append(rune, count);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString32Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString4096Bytes.</exception>
		public FixedString4096Bytes(ref FixedString32Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString32Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			unsafe
			{
				int len = 0;
				SafePtr dstBytes = GetSafePtr();
				SafePtr srcBytes = other.bytes.AsSafePtr();
				var srcLength = other.utf8LengthInBytes;
				var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
					srcLength);
				if (error != FormatError.None)
					return (int)error;
				this.Length = len;
			}

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString32Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public unsafe bool Equals(ref FixedString32Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString64Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString4096Bytes.</exception>
		public FixedString4096Bytes(ref FixedString64Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString64Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			unsafe
			{
				int len = 0;
				SafePtr dstBytes = GetSafePtr();
				SafePtr srcBytes = other.bytes.AsSafePtr();
				var srcLength = other.utf8LengthInBytes;
				var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
					srcLength);
				if (error != FormatError.None)
					return (int)error;
				this.Length = len;
			}

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString64Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public unsafe bool Equals(ref FixedString64Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString128Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString4096Bytes.</exception>
		public FixedString4096Bytes(ref FixedString128Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString128Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			unsafe
			{
				int len = 0;
				SafePtr dstBytes = GetSafePtr();
				SafePtr srcBytes = other.bytes.AsSafePtr();
				var srcLength = other.utf8LengthInBytes;
				var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
					srcLength);
				if (error != FormatError.None)
					return (int)error;
				this.Length = len;
			}

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString128Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public unsafe bool Equals(ref FixedString128Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString512Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString4096Bytes.</exception>
		public FixedString4096Bytes(ref FixedString512Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString512Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			unsafe
			{
				int len = 0;
				SafePtr dstBytes = GetSafePtr();
				SafePtr srcBytes = other.bytes.AsSafePtr();
				var srcLength = other.utf8LengthInBytes;
				var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
					srcLength);
				if (error != FormatError.None)
					return (int)error;
				this.Length = len;
			}

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString512Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public unsafe bool Equals(ref FixedString512Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns the lexicographical sort order of this string relative to another.
		/// </summary>
		/// <param name="other">A string to compare with.</param>
		/// <returns>A number denoting the lexicographical sort order of this string relative to the other:
		///
		/// 0 denotes that both strings have the same sort position.<br/>
		/// -1 denotes that this string should be sorted to precede the other.<br/>
		/// +1 denotes that this string should be sorted to follow the other.<br/>
		/// </returns>
		public int CompareTo(ref FixedString4096Bytes other)
		{
			return FixedStringExt.CompareTo(ref this, ref other);
		}

		/// <summary>
		/// Initializes and returns an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString4096Bytes.</exception>
		public FixedString4096Bytes(ref FixedString4096Bytes other)
		{
			this = default;
			var error = Initialize(ref other);
			CheckFormatError((FormatError)error);
		}

		/// <summary>
		/// Initializes an instance of FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="other">The string to copy.</param>
		/// <returns>zero on success, or non-zero on error.</returns>
		internal int Initialize(ref FixedString4096Bytes other)
		{
			bytes = default;
			utf8LengthInBytes = 0;
			unsafe
			{
				int len = 0;
				SafePtr dstBytes = GetSafePtr();
				SafePtr srcBytes = other.bytes.AsSafePtr();
				var srcLength = other.utf8LengthInBytes;
				var error = UTF8Ext.AppendUTF8Bytes(dstBytes, ref len, utf8MaxLengthInBytes, srcBytes,
					srcLength);
				if (error != FormatError.None)
					return (int)error;
				this.Length = len;
			}

			return 0;
		}

		/// <summary>
		/// Returns true if this string and another string are equal.
		/// </summary>
		/// <remarks>Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="other">A FixedString4096Bytes to compare for equality.</param>
		/// <returns>True if the two strings are equal.</returns>
		public unsafe bool Equals(ref FixedString4096Bytes other)
		{
			int alen = utf8LengthInBytes;
			int blen = other.utf8LengthInBytes;
			SafePtr aptr = bytes.AsSafePtr();
			SafePtr bptr = other.bytes.AsSafePtr();
			return UTF8Ext.EqualsUTF8Bytes(aptr, alen, bptr, blen);
		}

		/// <summary>
		/// Returns a new FixedString4096Bytes that is a copy of another string.
		/// </summary>
		/// <param name="b">A string to copy.</param>
		/// <returns>A new FixedString4096Bytes that is a copy of another string.</returns>
		/// <exception cref="IndexOutOfRangeException">Thrown if the string to copy's length exceeds the capacity of FixedString4096Bytes.</exception>
		public static implicit operator FixedString4096Bytes(string b) => new FixedString4096Bytes(b);

		/// <summary>
		/// Returns a new managed string that is a copy of this string.
		/// </summary>
		/// <returns>A new managed string that is a copy of this string.</returns>
		public override string ToString()
		{
			return this.ConvertToString();
		}

		/// <summary>
		/// Returns a hash code of this string.
		/// </summary>
		/// <remarks>Only the character bytes are included in the hash: any bytes beyond <see cref="Length"/> are not part of the hash.</remarks>
		/// <returns>The hash code of this string.</returns>
		public override int GetHashCode()
		{
			return this.ComputeHashCode();
		}

		/// <summary>
		/// Returns true if this string and an object are equal.
		/// </summary>
		/// <remarks>
		/// Returns false if the object is neither a System.String or a FixedString.
		///
		/// Two strings are equal if they have equal length and all their characters match.</remarks>
		/// <param name="obj">An object to compare for equality.</param>
		/// <returns>True if this string and the object are equal.</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj is string aString) return Equals(aString);
			if (obj is FixedString32Bytes aFixedString32Bytes) return Equals(aFixedString32Bytes);
			if (obj is FixedString64Bytes aFixedString64Bytes) return Equals(aFixedString64Bytes);
			if (obj is FixedString128Bytes aFixedString128Bytes) return Equals(aFixedString128Bytes);
			if (obj is FixedString512Bytes aFixedString512Bytes) return Equals(aFixedString512Bytes);
			if (obj is FixedString4096Bytes aFixedString4096Bytes) return Equals(aFixedString4096Bytes);
			return false;
		}

		[Conditional("DEBUG")]
		private void CheckIndexInRange(int index)
		{
			if (index < 0)
				throw new IndexOutOfRangeException($"Index {index} must be positive.");
			if (index >= utf8LengthInBytes)
				throw new IndexOutOfRangeException(
					$"Index {index} is out of range in FixedString4096Bytes of '{utf8LengthInBytes}' Length.");
		}

		[Conditional("DEBUG")]
		private void CheckLengthInRange(int length)
		{
			if (length < 0)
				throw new ArgumentOutOfRangeException($"Length {length} must be positive.");
			if (length > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Length {length} is out of range in FixedString4096Bytes of '{utf8MaxLengthInBytes}' Capacity.");
		}

		[Conditional("DEBUG")]
		private void CheckCapacityInRange(int capacity)
		{
			if (capacity > utf8MaxLengthInBytes)
				throw new ArgumentOutOfRangeException(
					$"Capacity {capacity} must be lower than {utf8MaxLengthInBytes}.");
		}

		[Conditional("DEBUG")]
		private static void CheckCopyError(CopyError error, string source)
		{
			if (error != CopyError.None)
				throw new ArgumentException($"FixedString4096Bytes: {error} while copying \"{source}\"");
		}

		[Conditional("DEBUG")]
		private static void CheckFormatError(FormatError error)
		{
			if (error != FormatError.None)
				throw new ArgumentException("Source is too long to fit into fixed string of this size");
		}
	}
}
