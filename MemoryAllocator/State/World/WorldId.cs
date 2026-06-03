using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.MemoryAllocator
{
	[StructLayout(LayoutKind.Explicit)]
	public struct WorldId : IEquatable<WorldId>
	{
		[FieldOffset(0)]
		// Для операций сравнения + HashCode
		private readonly int _raw;

		/// <summary>
		/// Индекс в списке миров у <see cref="WorldManager"/>.
		/// </summary>
		[FieldOffset(0)]
		public readonly ushort id;
		/// <summary>
		/// Всегда > 0, иначе невалидно.
		/// </summary>
		[FieldOffset(2)]
		public ushort version;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WorldId(int id, int version) : this((ushort)id, (ushort)version)
		{
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public WorldId(ushort id, ushort version)
		{
			this = default;
			this.id = id;
			this.version = version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator WorldId((ushort index, ushort id) indexId)
		{
			return new WorldId(indexId.index, indexId.id);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(WorldId a, WorldId b)
		{
			return a.version == b.version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(WorldId a, WorldId b)
		{
			return a.version != b.version;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override string ToString() => $"id: {id}, version: {version}";

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(WorldId other)
		{
			return _raw == other._raw;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override bool Equals(object obj)
		{
			return obj is WorldId other && Equals(other);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return _raw;
		}
	}
}
