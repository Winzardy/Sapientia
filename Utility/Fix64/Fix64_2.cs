using System;
using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unity.Mathematics;
#endif

namespace Sapientia.Deterministic
{
	/// <summary>
	/// Детерминированный 2D-вектор на Fix64. Аналог <c>float2</c>/<c>int2</c>, но без floating-point drift.
	/// Используется там где нужна побитовая воспроизводимость вычислений на разных машинах - ключи lookup-а,
	/// сеточные координаты, game-logic позиции.
	/// </summary>
	[Serializable]
	public struct Fix64_2 : IEquatable<Fix64_2>, IComparable<Fix64_2>
	{
		public Fix64 x;
		public Fix64 y;

		public static Fix64_2 Zero => default;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Fix64_2(Fix64 x, Fix64 y)
		{
			this.x = x;
			this.y = y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Fix64_2(Fix64 xy)
		{
			x = xy;
			y = xy;
		}

#if UNITY_5_3_OR_NEWER
		/// <summary>
		/// Вход в детерминированный домен из float-мира (физика, Unity-трансформы) - покомпонентно
		/// через <see cref="Fix64"/>, который сам implicit-конвертируется из float. Обратного
		/// implicit-каста намеренно нет: выход из Fix64 в float должен быть виден в коде.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Fix64_2(float2 value) => new(value.x, value.y);
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Fix64_2 operator +(Fix64_2 a, Fix64_2 b) => new(a.x + b.x, a.y + b.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Fix64_2 operator -(Fix64_2 a, Fix64_2 b) => new(a.x - b.x, a.y - b.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Fix64_2 operator *(Fix64_2 a, Fix64 b) => new(a.x * b, a.y * b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Fix64_2 operator *(Fix64 a, Fix64_2 b) => new(a * b.x, a * b.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Fix64_2 operator /(Fix64_2 a, Fix64 b) => new(a.x / b, a.y / b);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Fix64_2 operator -(Fix64_2 a) => new(-a.x, -a.y);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(Fix64_2 other) => x.Equals(other.x) && y.Equals(other.y);

		public int CompareTo(Fix64_2 other)
		{
			var result = x.CompareTo(other.x);
			return result != 0 ? result : y.CompareTo(other.y);
		}

		public override bool Equals(object obj) => obj is Fix64_2 other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				return (x.GetHashCode() * 397) ^ y.GetHashCode();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(Fix64_2 left, Fix64_2 right) => left.Equals(right);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(Fix64_2 left, Fix64_2 right) => !left.Equals(right);

		public override string ToString() => $"({x}, {y})";
	}
}
