using System;
using System.Runtime.CompilerServices;
using Submodules.Sapientia.Data;

namespace Sapientia.TypeIndexer
{
	public struct TypeId : IEquatable<TypeId>
	{
		public static readonly TypeId Empty = default;

		internal Id id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeId Create<T>()
		{
			return TypeIdOf<T>.typeId;
		}

		public Type Type
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => IndexedTypes.GetType(this);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(TypeId typeId)
		{
			return typeId.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator TypeId(int id)
		{
			return new TypeId { id = id, };
		}

		public static bool operator ==(TypeId a, TypeId b)
		{
			return a.id == b.id;
		}

		public static bool operator !=(TypeId a, TypeId b)
		{
			return a.id != b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(TypeId other)
		{
			return id == other.id;
		}

		public override bool Equals(object obj)
		{
			return obj is TypeId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}

	public struct TypeId<TContext> : IEquatable<TypeId<TContext>>
	{
		public static readonly TypeId<TContext> Empty = default;
		public static readonly int Count;

		internal Id id;

		static TypeId()
		{
			Count = IndexedTypes.GetContextCount(typeof(TContext));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(TypeId<TContext> typeId)
		{
			return typeId.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator TypeId<TContext>(int index)
		{
			return new TypeId<TContext> { id = index, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator TypeId(TypeId<TContext> typeId)
		{
			return new TypeId { id = typeId.id, };
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator TypeId<TContext>(TypeId typeId)
		{
			return new TypeId<TContext> { id = typeId.id, };
		}

		public static bool operator ==(TypeId<TContext> a, TypeId<TContext> b)
		{
			return a.id == b.id;
		}

		public static bool operator !=(TypeId<TContext> a, TypeId<TContext> b)
		{
			return a.id != b.id;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(TypeId<TContext> other)
		{
			return id == other.id;
		}

		public override bool Equals(object obj)
		{
			return obj is TypeId<TContext> other && Equals(other);
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}
}
