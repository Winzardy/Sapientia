using System;
using System.Runtime.CompilerServices;

namespace Submodules.Sapientia.Data
{
	public struct Id : IEquatable<Id>, IComparable<Id>
	{
		public static readonly Id Invalid = new Id
		{
			id = 0,
		};

		public int id;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => id > 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(Id nodeId)
		{
			return nodeId.id - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Id(int id)
		{
			return new Id { id = id + 1, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Id other)
		{
			return id == other.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Id a, Id b)
		{
			return a.id == b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Id a, Id b)
		{
			return a.id != b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Id operator +(Id a, int b)
		{
			return new Id
			{
				id = a.id + b,
			};
		}

		public int CompareTo(Id other)
		{
			return id.CompareTo(other.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is Id other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return id;
		}
	}
}
