using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Content
{
	[Serializable]
	[StructLayout(LayoutKind.Explicit)]
	public partial struct SerializableGuid : IEquatable<SerializableGuid>
	{
		private const string GUID_STRING_FORMAT = "N";

		[FieldOffset(0)]
		[NonSerialized]
		public Guid guid;

		[FieldOffset(0)]
		public long low;

		[FieldOffset(8)]
		public long high;

		public static SerializableGuid Empty = Guid.Empty;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SerializableGuid(long low, long high) : this()
		{
			this.low = low;
			this.high = high;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SerializableGuid(Guid guid) : this()
		{
			this.guid = guid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SerializableGuid(string guid, string format = GUID_STRING_FORMAT) : this()
		{
			if (guid.IsNullOrEmpty())
			{
				this.guid = Guid.Empty;
				return;
			}

			this.guid = !format.IsNullOrEmpty()
				? Guid.ParseExact(guid, format)
				: Guid.Parse(guid);
		}

		public bool Equals(SerializableGuid other) => guid.Equals(other.guid);

		public override bool Equals(object obj) => obj is SerializableGuid other && Equals(other);

		public override int GetHashCode() => guid.GetHashCode();

		public static SerializableGuid New() => Guid.NewGuid();

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsEmpty() => guid == Guid.Empty;

		public static bool TryParse(string str, out SerializableGuid guid)
		{
			guid = Empty;

			if (!Guid.TryParse(str, out var g))
				return false;

			guid = g;
			return true;
		}

		public static SerializableGuid Parse(string str) => Guid.Parse(str);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => guid.ToString(GUID_STRING_FORMAT);
	}
}
