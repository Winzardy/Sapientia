using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Collections.ByteReader;

namespace Sapientia.Serializers
{
	public static unsafe class GenericBlittableSerializer
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Serialize<T>(this Span<byte> span, T value) where T : unmanaged
		{
			MemoryMarshal.Write(span, ref value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Deserialize<T>(this Span<byte> span) where T : unmanaged
		{
			return Deserialize<T>((ReadOnlySpan<byte>)span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Deserialize<T>(this ReadOnlySpan<byte> span) where T : unmanaged
		{
			return MemoryMarshal.Read<T>(span);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Push<T>(this ByteReader reader, T value) where T : unmanaged
		{
			Serialize<T>(reader.AllocateData(sizeof(T)), value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Pop<T>(this ByteReader reader) where T : unmanaged
		{
			return Deserialize<T>(reader.PopData(sizeof(T)));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T Dequeue<T>(this ByteReader reader) where T : unmanaged
		{
			return Deserialize<T>(reader.DequeueData(sizeof(T)));
		}

		public static T Receive<T>(this ref Span<byte> span) where T : unmanaged
		{
			var valueSlice = span.Slice(0, sizeof(T));
			span = span.Slice(sizeof(T), span.Length - sizeof(T));
			return valueSlice.Deserialize<T>();
		}

		public static T Receive<T>(this Socket socket) where T : unmanaged
		{
			Span<byte> span = stackalloc byte[sizeof(T)];
			socket.Receive(span);
			return span.Deserialize<T>();
		}
	}
}