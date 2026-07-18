using System;
using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unity.Mathematics;
#endif

namespace Sapientia.Data
{
	/// <summary>
	/// Компактный вектор 2 x short (4 байта) в стиле Unity.Mathematics - для упакованных данных,
	/// где int2 избыточен (бинарные кеши, сериализуемые пулы). Без паддинга, alignment 2.
	/// </summary>
	public struct short2 : IEquatable<short2>
	{
		public short x;
		public short y;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public short2(short x, short y)
		{
			this.x = x;
			this.y = y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(short2 other) => x == other.x && y == other.y;

		public override bool Equals(object obj) => obj is short2 other && Equals(other);

		public override int GetHashCode() => (ushort)x | y << 16;

		public override string ToString() => $"({x}, {y})";

		public static bool operator ==(short2 left, short2 right) => left.Equals(right);
		public static bool operator !=(short2 left, short2 right) => !left.Equals(right);

#if UNITY_5_3_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int2(short2 value) => new int2(value.x, value.y);

		/// <summary>Явное сужение: вызывающий отвечает за диапазон значений.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator short2(int2 value) => new short2((short)value.x, (short)value.y);
#endif
	}
}
