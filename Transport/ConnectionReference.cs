using System.Runtime.CompilerServices;

namespace Sapientia.Transport
{
	public readonly struct ConnectionReference
	{
		public static readonly ConnectionReference empty = new(-1, -1, -1);

		public readonly int index;
		public readonly int id;
		public readonly int customId;

		public ConnectionReference(int index, int id, int customId)
		{
			this.index = index;
			this.id = id;
			this.customId = customId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(ConnectionReference a, ConnectionReference b)
		{
			return a.index == b.index & a.id == b.id & a.customId == b.customId;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(ConnectionReference a, ConnectionReference b)
		{
			return a.index != b.index | a.id != b.id | a.customId != b.customId;
		}
	}
}