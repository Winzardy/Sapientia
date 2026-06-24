using System;
using System.Runtime.InteropServices;
using Submodules.Sapientia.Data;

namespace Sapientia.Data
{
	/// <summary>
	/// Ключ версии сущности — пара <c>(Id; id, version)</c>. <c>version</c> работает как generation
	/// (как у <c>Entity</c>): ссылка биндится к конкретной версии, поэтому stale-версия детектируется, а
	/// переиспользование <c>id</c> безопасно. Обобщённый вариант рядом с <see cref="Id"/>.
	/// </summary>
	/// <remarks>Раскладка — <b>Sequential</b> (<c>id</c>@0, <c>version</c>@4 = 8 байт): generic-структуры с
	/// <c>[StructLayout(Explicit)]</c> запрещены под Burst (BC1047), а эта пара достижима из <c>[BurstCompile]</c>
	/// диспатча (<c>NodeContext</c> → <c>CompiledBlueprintHeader.blueprintKey</c>). Equality/hash — по полям
	/// (<c>id</c>+<c>version</c>); байтовая раскладка совпадает с прежним explicit-вариантом (overlay-<c>_raw</c> убран).</remarks>
	[StructLayout(LayoutKind.Explicit)]
	public readonly struct VersionedId : IEquatable<VersionedId>
	{
		[FieldOffset(0)]
		private readonly long _raw;
		[FieldOffset(0)]
		public readonly Id id;
		[FieldOffset(4)]
		public readonly Id version;

		public VersionedId(Id id, Id version)
		{
			this._raw = default;
			this.id = id;
			this.version = version;
		}

		public bool Equals(VersionedId other)
		{
			return _raw == other._raw;
		}

		public override bool Equals(object? obj)
		{
			return obj is VersionedId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _raw.GetHashCode();
		}

		public static bool operator ==(VersionedId a, VersionedId b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(VersionedId a, VersionedId b)
		{
			return !a.Equals(b);
		}
	}
}
