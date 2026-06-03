namespace Sapientia.Extensions
{
	public static class BitwiseExtensions
	{
		public static ulong SetBitValue(this ulong bitmask, int position, bool value)
		{
			return value ?
				bitmask.SetBitTrue(position) :
				bitmask.SetBitFalse(position);
		}

		public static ulong SetBitTrue(this ulong bitmask, int position)
		{
			return bitmask |= (1UL << position);
		}

		public static ulong SetBitFalse(this ulong bitmask, int position)
		{
			return bitmask &= ~(1UL << position);
		}

		public static ulong ToggleBit(this ulong bitmask, int position)
		{
			return bitmask ^= (1UL << position);
		}

		public static bool IsBitTrue(this ulong bitmask, int position)
		{
			return (bitmask & (1UL << position)) != 0;
		}

		public static bool AllBitsTrue(this ulong bitmask, int size)
		{
			var x = (1UL << size) - 1;
			return (bitmask & x) == x;
		}

		public static bool AllBitsFalse(this ulong bitmask, int size)
		{
			return (bitmask & ((1UL << size) - 1)) == 0;
		}

		public static string ToBinaryString(this ulong source, bool padding = true)
		{
			var s = System.Convert.ToString((long)source, 2);
			if (padding)
			{
				s = s.PadLeft(8, '0');
			}

			return s;
		}

		public static string ToBinaryString(this ulong source, int size)
		{
			var s = System.Convert.ToString((long)source, 2);
			s = s.PadLeft(size > 0 ? size : 8, '0');

			return s;
		}
	}
}
