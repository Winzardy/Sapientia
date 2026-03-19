using System;
using System.Runtime.CompilerServices;

namespace Sapientia.TypeIndexer
{
	public static class TypeId<T>
	{
		public static readonly TypeId typeId;
		public static readonly int Count;

		static TypeId()
		{
			IndexedTypes.GetTypeIndex(typeof(T), out typeId);
			Count = IndexedTypes.GetContextCount(typeof(T));
		}
	}

	public static class TypeIndex<TContext, TType>
		where TType : IIndexedType
	{
		public static readonly TypeIndex<TContext> typeIndex;

		static TypeIndex()
		{
			IndexedTypes.GetContextTypeIndex(typeof(TContext), typeof(TType), out int idx);
			typeIndex = idx;
		}
	}

	public struct TypeId : IEquatable<TypeId>
	{
		public static readonly TypeId Empty = -1;

		internal int id;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static TypeId Create<T>()
		{
			return TypeId<T>.typeId;
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
			return id;
		}
	}

	public struct TypeIndex<TContext> : IEquatable<TypeIndex<TContext>>
	{
		public static readonly TypeIndex<TContext> Empty = -1;
		public static readonly int Count;

		internal int index;

		static TypeIndex()
		{
			Count = IndexedTypes.GetContextCount(typeof(TContext));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator int(TypeIndex<TContext> typeIndex)
		{
			return typeIndex.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator TypeIndex<TContext>(int index)
		{
			return new TypeIndex<TContext> { index = index, };
		}

		public static bool operator ==(TypeIndex<TContext> a, TypeIndex<TContext> b)
		{
			return a.index == b.index;
		}

		public static bool operator !=(TypeIndex<TContext> a, TypeIndex<TContext> b)
		{
			return a.index != b.index;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(TypeIndex<TContext> other)
		{
			return index == other.index;
		}

		public override bool Equals(object obj)
		{
			return obj is TypeIndex<TContext> other && Equals(other);
		}

		public override int GetHashCode()
		{
			return index;
		}
	}
}
