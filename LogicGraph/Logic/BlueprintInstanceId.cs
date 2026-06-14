using System;
using System.Runtime.InteropServices;
using Submodules.Sapientia.Data;

namespace Sapientia.LogicGraph
{
	/// <summary>
	/// Хендл живого инстанса в пределах одного <see cref="BlueprintInstanceStorage"/>: индекс слота +
	/// <b>generation</b>. Так как сторедж переиспользует слоты (slot-recycling), generation защищает от
	/// <b>stale</b>-хендлов: при освобождении слота его generation инкрементируется, и старый хендл на тот же
	/// индекс перестаёт резолвиться (паттерн <c>Entity.generation</c>). <c>generation == 0</c> — невалидный хендл.
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct BlueprintInstanceId : IEquatable<BlueprintInstanceId>
	{
		[FieldOffset(0)]
		public Id<BlueprintInstanceHeader> id;
		[FieldOffset(4)]
		public int generation;

		[FieldOffset(0)]
		private readonly long _raw;

		public bool IsValid => generation != 0;

		public static readonly BlueprintInstanceId Invalid = default;

		public bool Equals(BlueprintInstanceId other)
		{
			return _raw == other._raw;
		}

		public override bool Equals(object obj)
		{
			return obj is BlueprintInstanceId other && Equals(other);
		}

		public override int GetHashCode()
		{
			return _raw.GetHashCode();
		}

		public static bool operator ==(BlueprintInstanceId a, BlueprintInstanceId b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(BlueprintInstanceId a, BlueprintInstanceId b)
		{
			return !a.Equals(b);
		}
	}
}
