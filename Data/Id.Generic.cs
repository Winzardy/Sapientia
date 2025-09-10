using System;
using System.Runtime.CompilerServices;

namespace Submodules.Sapientia.Data
{
	public struct Id<T> : IEquatable<Id<T>>
	{
		public static readonly Id<T> Invalid = new Id<T>
		{
			id = Id.Invalid,
		};

		public Id id;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => id.IsValid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Id<T> other)
		{
			return id == other.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Id<T> a, Id<T> b)
		{
			return a.id == b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Id<T> a, Id<T> b)
		{
			return a.id != b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(Id<T> genericId)
		{
			return genericId.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Id<T>(int id)
		{
			return new Id<T> { id = id, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Id(Id<T> genericId)
		{
			return genericId.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Id<T>(Id id)
		{
			return new Id<T> { id = id, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Id<T> operator +(Id<T> a, int b)
		{
			return new Id<T>
			{
				id = a.id + b,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Id<T> operator -(Id<T> a, int b)
		{
			return new Id<T>
			{
				id = a.id - b,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is Id<T> other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}
}
