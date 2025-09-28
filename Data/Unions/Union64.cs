using System.Runtime.InteropServices;

namespace Submodules.Sapientia.Data.Convertors
{
	[StructLayout(LayoutKind.Explicit)]
	public unsafe struct Union64
	{
		[FieldOffset(0)]
		private fixed byte _bytes[8];
		[FieldOffset(0)]
		private fixed short _shorts[4];
		[FieldOffset(0)]
		private fixed ushort _ushorts[4];
		[FieldOffset(0)]
		private fixed int _ints[2];
		[FieldOffset(0)]
		private fixed uint _uints[2];
		[FieldOffset(0)]
		private fixed long _longs[1];
		[FieldOffset(0)]
		private fixed ulong _ulongs[1];

		public Union64(byte byte1, byte byte2 = 0, byte byte3 = 0, byte byte4 = 0, byte byte5 = 0, byte byte6 = 0, byte byte7 = 0, byte byte8 = 0)
		{
			_bytes[0] = byte1;
			_bytes[1] = byte2;
			_bytes[2] = byte3;
			_bytes[3] = byte4;
			_bytes[4] = byte5;
			_bytes[5] = byte6;
			_bytes[6] = byte7;
			_bytes[7] = byte8;
		}

		public Union64(short short1, short short2 = 0, short short3 = 0, short short4 = 0)
		{
			_shorts[0] = short1;
			_shorts[1] = short2;
			_shorts[2] = short3;
			_shorts[3] = short4;
		}

		public Union64(ushort ushort1, ushort ushort2 = 0, ushort ushort3 = 0, ushort ushort4 = 0)
		{
			_ushorts[0] = ushort1;
			_ushorts[1] = ushort2;
			_ushorts[2] = ushort3;
			_ushorts[3] = ushort4;
		}

		public Union64(int int1, int int2 = 0)
		{
			_ints[0] = int1;
			_ints[1] = int2;
		}

		public Union64(uint uint1, uint uint2 = 0)
		{
			_uints[0] = uint1;
			_uints[1] = uint2;
		}

		public Union64(long long1)
		{
			_longs[0] = long1;
		}

		public Union64(ulong ulong1)
		{
			_ulongs[0] = ulong1;
		}

		public static implicit operator long(Union64 union)
		{
			return union._longs[0];
		}

		public static implicit operator ulong(Union64 union)
		{
			return union._ulongs[0];
		}
	}
}
