using System.Runtime.CompilerServices;

namespace Fusumity.Transport
{
	public readonly struct ConnectionReference
	{
		public static readonly ConnectionReference empty = new(-1, -1);

		public readonly int index;
		public readonly int id;

		public ConnectionReference(int index, int id)
		{
			this.index = index;
			this.id = id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(ConnectionReference a, ConnectionReference b)
		{
			return a.index == b.index & a.id == b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(ConnectionReference a, ConnectionReference b)
		{
			return a.index != b.index | a.id != b.id;
		}
	}
}