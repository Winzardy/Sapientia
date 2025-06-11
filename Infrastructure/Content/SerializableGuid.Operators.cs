using System;
using System.Runtime.CompilerServices;

namespace Content
{
	public partial struct SerializableGuid
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Guid(SerializableGuid serializableGuid) => serializableGuid.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SerializableGuid(Guid guid) => new(guid);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator string(in SerializableGuid guid) => guid.ToString();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SerializableGuid a, string b) => a.guid.ToString() == b;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SerializableGuid a, string b) => !(a == b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(string a, SerializableGuid b) => b == a;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(string a, SerializableGuid b) => !(b == a);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SerializableGuid a, Guid b) => a.guid == b;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SerializableGuid a, Guid b) => !(a == b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Guid a, SerializableGuid b) => b == a;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Guid a, SerializableGuid b) => !(b == a);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SerializableGuid a, SerializableGuid b) => a.guid == b.guid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SerializableGuid a, SerializableGuid b) => a.guid != b.guid;
	}
}
