using System.Runtime.InteropServices;

namespace Submodules.Sapientia.Data.Convertors
{
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct Union32
	{
		[FieldOffset(0)]
		private fixed byte _bytes[4];
		[FieldOffset(0)]
		private fixed short _shorts[2];
		[FieldOffset(0)]
		private fixed ushort _ushorts[2];
		[FieldOffset(0)]
		private fixed int _ints[1];
		[FieldOffset(0)]
		private fixed uint _uints[1];

		public Union32(byte byte1, byte byte2 = 0, byte byte3 = 0, byte byte4 = 0)
		{
			_bytes[0] = byte1;
			_bytes[1] = byte2;
			_bytes[2] = byte3;
			_bytes[3] = byte4;
		}

		public Union32(short short1, short short2 = 0)
		{
			_shorts[0] = short1;
			_shorts[1] = short2;
		}

		public Union32(ushort ushort1, ushort ushort2 = 0)
		{
			_ushorts[0] = ushort1;
			_ushorts[1] = ushort2;
		}

		public Union32(int int1)
		{
			_ints[0] = int1;
		}

		public Union32(uint uint1)
		{
			_uints[0] = uint1;
		}

		public static implicit operator int(Union32 union)
		{
			return union._ints[0];
		}

		public static implicit operator uint(Union32 union)
		{
			return union._uints[0];
		}
	}
}
