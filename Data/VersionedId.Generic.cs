using System;
using System.Runtime.InteropServices;
using Submodules.Sapientia.Data;

namespace Sapientia.Data
{
	/// <summary>
	/// Ключ версии сущности — пара <c>(Id&lt;T&gt; id, version)</c>. <c>version</c> работает как generation
	/// (как у <c>Entity</c>): ссылка биндится к конкретной версии, поэтому stale-версия детектируется, а
	/// переиспользование <c>id</c> безопасно. Обобщённый вариант рядом с <see cref="Id{T}"/>.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct VersionedId<T> : IEquatable<VersionedId<T>>
	{
		[FieldOffset(0)]
		public readonly Id<T> id;
		[FieldOffset(4)]
		public readonly Id version;

		[FieldOffset(0)]
		private readonly long _raw;

		public VersionedId(Id<T> id, Id version)
		{
			_raw = default;
			this.id = id;
			this.version = version;
		}

		public bool Equals(VersionedId<T> other)
		{
			return _raw == other._raw;
		}

		public override bool Equals(object? obj)
		{
			return obj is VersionedId<T> other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _raw.GetHashCode();
		}

		public static bool operator ==(VersionedId<T> a, VersionedId<T> b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(VersionedId<T> a, VersionedId<T> b)
		{
			return !a.Equals(b);
		}
	}
}
