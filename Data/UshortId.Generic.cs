using System;
using System.Runtime.CompilerServices;

namespace Submodules.Sapientia.Data
{
	[Serializable]
	public struct UshortId<T> : IEquatable<UshortId<T>>
	{
		public static readonly UshortId<T> Invalid = new UshortId<T>
		{
			id = UshortId.Invalid,
		};

		public UshortId id;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => id.IsValid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(UshortId<T> other)
		{
			return id == other.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(UshortId<T> a, UshortId<T> b)
		{
			return a.id == b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(UshortId<T> a, UshortId<T> b)
		{
			return a.id != b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator ushort(UshortId<T> genericId)
		{
			return genericId.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UshortId<T>(ushort id)
		{
			return new UshortId<T> { id = id, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UshortId<T>(int id)
		{
			return new UshortId<T> { id = (ushort)id, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UshortId(UshortId<T> genericId)
		{
			return genericId.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator UshortId<T>(UshortId id)
		{
			return new UshortId<T> { id = id, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UshortId<T> operator +(UshortId<T> a, ushort b)
		{
			return new UshortId<T>
			{
				id = a.id + b,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UshortId<T> operator -(UshortId<T> a, ushort b)
		{
			return new UshortId<T>
			{
				id = a.id - b,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is UshortId<T> other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		public override string ToString()
		{
			return id.ToString();
		}
	}
}
