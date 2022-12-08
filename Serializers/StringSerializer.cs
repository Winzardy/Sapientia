using System;
using System.Runtime.CompilerServices;
using System.Text;
using Sapientia.Collections.ByteReader;

namespace Sapientia.Serializers
{
	public static class StringSerializer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ReadOnlySpan<byte> Serialize_String(this string value, out int byteLenght)
		{
			var bytes = new ReadOnlySpan<byte>(Encoding.ASCII.GetBytes(value));
			byteLenght = bytes.Length;
			return bytes;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Serialize_String(this Span<byte> span, string value)
		{
			var bytes = value.Serialize_String(out var bytesLength);
			var ushortBytesLength = (ushort)bytesLength;

			// Serialize header
			span.Serialize(ushortBytesLength);
			// Serialize string
			var stringSlice = span.Slice(sizeof(ushort), bytesLength);
			bytes.CopyTo(stringSlice);
			// Serialize ender
			var enderSlice = span.Slice(sizeof(ushort) + bytesLength, sizeof(ushort));
			enderSlice.Serialize(ushortBytesLength);

			return bytesLength + sizeof(ushort) * 2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Dequeue_String(this Span<byte> span, out int bytesCount)
		{
			return Dequeue_String((ReadOnlySpan<byte>)span, out bytesCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Pop_String(this Span<byte> span, out int bytesCount)
		{
			return Pop_String((ReadOnlySpan<byte>)span, out bytesCount);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Dequeue_String(this ReadOnlySpan<byte> span, out int bytesCount)
		{
			bytesCount = span.Deserialize<ushort>();

			var stringSlice = span.Slice(sizeof(ushort), bytesCount);

			bytesCount += sizeof(ushort) * 2;
			return Encoding.ASCII.GetString(stringSlice);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Pop_String(this ReadOnlySpan<byte> span, out int bytesCount)
		{
			var endingSlice = span.Slice(span.Length - sizeof(ushort), sizeof(ushort));
			bytesCount = endingSlice.Deserialize<ushort>();

			var stringSlice = span.Slice(span.Length - sizeof(ushort) - bytesCount, bytesCount);

			bytesCount += sizeof(ushort) * 2;
			return Encoding.ASCII.GetString(stringSlice);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Push_String(this ByteReader reader, string value)
		{
			var bytes = new ReadOnlySpan<byte>(Encoding.ASCII.GetBytes(value));
			var bytesLength = bytes.Length;
			var ushortBytesLength = (ushort)bytesLength;

			var span = reader.AllocateData(bytesLength + sizeof(ushort) * 2);

			// Serialize header
			span.Serialize(ushortBytesLength);
			// Serialize string
			var stringSlice = span.Slice(sizeof(ushort), bytesLength);
			bytes.CopyTo(stringSlice);
			// Serialize ender
			var enderSlice = span.Slice(sizeof(ushort) + bytesLength, sizeof(ushort));
			enderSlice.Serialize(ushortBytesLength);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Pop_String(this ByteReader reader)
		{
			var length = reader.Pop<ushort>();
			var stringSpan = reader.PopData(length);
			reader.Pop<ushort>();

			return Encoding.ASCII.GetString(stringSpan);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Dequeue_String(this ByteReader reader)
		{
			var length = reader.Dequeue<ushort>();
			var stringSpan = reader.DequeueData(length);
			reader.Dequeue<ushort>();

			return Encoding.ASCII.GetString(stringSpan);
		}
	}
}