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
	/// <remarks>Раскладка — <b>Sequential</b> (<c>id</c>@0, <c>version</c>@4 = 8 байт): generic-структуры с
	/// <c>[StructLayout(Explicit)]</c> запрещены под Burst (BC1047), а эта пара достижима из <c>[BurstCompile]</c>
	/// диспатча (<c>NodeContext</c> → <c>CompiledBlueprintHeader.blueprintKey</c>). Equality/hash — по полям
	/// (<c>id</c>+<c>version</c>); байтовая раскладка совпадает с прежним explicit-вариантом (overlay-<c>_raw</c> убран).</remarks>
	public readonly struct VersionedId<T> : IEquatable<VersionedId<T>>
	{
		private readonly VersionedId _versionedId;

		public Id Id => _versionedId.id;
		public Id Version => _versionedId.version;

		public VersionedId(Id<T> id, Id version)
		{
			this._versionedId = new VersionedId(id, version);
		}

		public bool Equals(VersionedId<T> other)
		{
			return _versionedId == other._versionedId;
		}

		public override bool Equals(object? obj)
		{
			return obj is VersionedId<T> other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _versionedId.GetHashCode();
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
