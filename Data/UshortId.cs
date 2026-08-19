using System;
using System.Runtime.CompilerServices;

namespace Submodules.Sapientia.Data
{
	[Serializable]
	public struct UshortId : IEquatable<UshortId>, IComparable<UshortId>
	{
		public static readonly UshortId Invalid = new UshortId
		{
			id = 0,
		};

		public ushort id;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => id > 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ushort(UshortId nodeId)
		{
			return (ushort)(nodeId.id - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UshortId(ushort id)
		{
			return new UshortId { id = (ushort)(id + 1), };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(UshortId other)
		{
			return id == other.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(UshortId a, UshortId b)
		{
			return a.id == b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(UshortId a, UshortId b)
		{
			return a.id != b.id;
		}

		/// <summary>
		/// Сравнение идёт по СЫРОМУ id, а не по значению: без этих операторов сравнение двух UshortId
		/// уезжало бы в неявную конверсию в ushort, которая вычитает единицу, и <see cref="Invalid"/>
		/// (сырой 0) превращался бы в 65535 — то есть в «больше всех». Порядок валидных id от сдвига
		/// на единицу не меняется, а Invalid встаёт туда, где ему и место: ниже любого валидного.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >(UshortId a, UshortId b)
		{
			return a.id > b.id;
		}

		/// <inheritdoc cref="op_GreaterThan"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <(UshortId a, UshortId b)
		{
			return a.id < b.id;
		}

		/// <inheritdoc cref="op_GreaterThan"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator >=(UshortId a, UshortId b)
		{
			return a.id >= b.id;
		}

		/// <inheritdoc cref="op_GreaterThan"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator <=(UshortId a, UshortId b)
		{
			return a.id <= b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UshortId operator +(UshortId a, ushort b)
		{
			return new UshortId
			{
				id = (ushort)(a.id + b),
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UshortId operator -(UshortId a, ushort b)
		{
			return new UshortId
			{
				id = (ushort)(a.id - b),
			};
		}

		public int CompareTo(UshortId other)
		{
			return id.CompareTo(other.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return id;
		}

		public override string ToString()
		{
			return ((ushort)this).ToString();
		}
	}
}
