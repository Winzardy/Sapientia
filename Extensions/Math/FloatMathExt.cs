using System;
using System.Globalization;
using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unity.Burst;
using Unity.Mathematics;
#endif

namespace Sapientia.Extensions
{
	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#de24076fb1f44a2795403edc13914eb0
	/// </summary>
	public static class FloatMathExt
	{
		public const float EPSILON = 0.0000001f;
		// The well-known 3.14159265358979...
		public const float PI = 3.141593f;
		public const float HALF_PI = PI / 2f;
		public const float TWO_PI = PI * 2f;
		public const float INV_TWO_PI = 1f / TWO_PI;
		public const float DEG_TO_RAD = PI / 180f;
		public const float RAD_TO_DEG = 180f / PI;
		// 0.00000001f - 1f = -1f
		// 0.0000001f - 1f = -0.9999999f
		private const float NEGATIVE_FLOOR_OFFSET = EPSILON - 1f;
		// 1f - 0.00000001f = 1f
		// 1f - 0.0000001f = 0.9999999f
		private const float POSITIVE_CEIL_OFFSET = 1f - EPSILON;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Pow(this float value, float power)
		{
#if UNITY_5_3_OR_NEWER
			return math.pow(value, power);
#else
			return MathF.Pow(value, power);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DivRem(this float value, float divider, out float remainder)
		{
			remainder = value % divider;
			return (int)((value - remainder) / divider);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ToRad(this float value)
		{
			return value * DEG_TO_RAD;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float ToDeg(this float value)
		{
			return value * RAD_TO_DEG;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CeilToInt_Negative(this float value)
		{
			return (int)value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CeilToInt_Positive(this float value)
		{
			return (int)(value + POSITIVE_CEIL_OFFSET);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CeilToInt(this float value)
		{
			return value < 0 ? (int)value : (int)(value + POSITIVE_CEIL_OFFSET);
		}

		// ~70% faster: Mathf.FloorToInt
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FloorToInt_Negative(this float value)
		{
			return (int)(value + NEGATIVE_FLOOR_OFFSET);
		}

		// ~70% faster: Mathf.FloorToInt
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FloorToInt_Positive(this float value)
		{
			return (int)value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int FloorToInt(this float value)
		{
			return value >= 0 ? (int)value : (int)(value + NEGATIVE_FLOOR_OFFSET);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Floor_Negative(this float value, float round)
		{
			return (value / round).FloorToInt_Negative() * round;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Floor_Positive(this float value, float round)
		{
			return (value / round).FloorToInt_Positive() * round;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Floor(this float value)
		{
#if UNITY_5_3_OR_NEWER
			return (int)math.floor(value);
#else
			return (int)MathF.Floor(value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Floor(this float value, float round)
		{
			return (value / round).Floor() * round;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RoundToInt(this float value)
		{
#if UNITY_5_3_OR_NEWER
			return (int)math.round(value);
#else
			return (int)MathF.Round(value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int RoundToInt(this float value, int digits)
		{
			return (int)MathF.Round(value, digits);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Round(this float value, int digits)
		{
			return MathF.Round(value, digits);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe int Sign(this float value)
		{
			return (((*(int*)&value) & int.MinValue) >> IntMathExt.FIRST_TO_LAST_SHIFT) | 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe float Abs(this float value)
		{
#if UNITY_5_3_OR_NEWER
			return math.abs(value);
#else
			var uintValue = (*(uint*)&value) & 0x7FFFFFFF;
			return *(float*)&uintValue;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsApproximatelyZero(this float value)
		{
			return value.Abs() < EPSILON;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsApproximatelyOne(this float value)
		{
			return (value - 1).Abs() < EPSILON;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsApproximatelyEqual(this float value, float other)
		{
			return (value - other).Abs() < EPSILON;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsInRange(this float value, float min, float max)
		{
			return value >= min && value <= max;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp(this float value, float min, float max)
		{
#if UNITY_5_3_OR_NEWER
			return math.clamp(value, min, max);
#else
			return value < min ? min : (value > max ? max : value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Clamp01(this float value)
		{
			return value < 0f ? 0f : (value > 1f ? 1f : value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Max(this float a, float b)
		{
#if UNITY_5_3_OR_NEWER
			return math.max(a, b);
#else
			return a > b ? a : b;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Max(this float a, float b, float c, float d)
		{
#if UNITY_5_3_OR_NEWER
			return math.max(math.max(a, b), math.max(c, d));
#else
			var abMax = a > b ? a : b;
			var cdMax = c > d ? c : d;
			return abMax > cdMax ? abMax : cdMax;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Min(this float a, float b)
		{
#if UNITY_5_3_OR_NEWER
			return math.min(a, b);
#else
			return a < b ? a : b;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Min(this float a, float b, float c, float d)
		{
#if UNITY_5_3_OR_NEWER
			return math.min(math.min(a, b), math.min(c, d));
#else
			var abMin = a < b ? a : b;
			var cdMin = c < d ? c : d;
			return abMin < cdMin ? abMin : cdMin;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqrt(this float value)
		{
#if UNITY_5_3_OR_NEWER
			return math.sqrt(value);
#else
			return MathF.Sqrt(value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sqr(this float value)
		{
			return value * value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Cos(this float rad)
		{
#if UNITY_5_3_OR_NEWER
			return math.cos(rad);
#else
			return MathF.Cos(rad);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Acos(this float cos)
		{
#if UNITY_5_3_OR_NEWER
			return math.acos(cos);
#else
			return MathF.Acos(cos);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sin(this float rad)
		{
#if UNITY_5_3_OR_NEWER
			return math.sin(rad);
#else
			return MathF.Sin(rad);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void SinCos(this float rad, out float sin, out float cos)
		{
#if UNITY_5_3_OR_NEWER
			math.sincos(rad, out sin, out cos);
#else
			sin = rad.Sin();
			cos = rad.Cos();
#endif
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Asin(this float sin)
		{
#if UNITY_5_3_OR_NEWER
			return math.asin(sin);
#else
			return MathF.Asin(sin);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Tan(this float rad)
		{
#if UNITY_5_3_OR_NEWER
			return math.tan(rad);
#else
			return MathF.Tan(rad);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Atan2(this float sin, float cos)
		{
#if UNITY_5_3_OR_NEWER
			return math.atan2(sin, cos);
#else
			return MathF.Atan2(sin, cos);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Atan(this float tan)
		{
#if UNITY_5_3_OR_NEWER
			return math.atan(tan);
#else
			return MathF.Atan(tan);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Lerp(this float current, float target, float coefficient)
		{
			return current + (target - current) * coefficient;
		}

		/// <returns> Returns Lerp's coefficient</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float UnLerp(this float current, float target, float value)
		{
			return (value - current) / (target - current);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SmoothStep(this float current, float target, float step)
		{
			var distance = target - current;
			var sign = distance.Sign();

			return distance * sign > step ? current + step * sign : target;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DeltaRad(this float current, float target)
		{
			var distance = target - current;
			var radDistance = Clamp(distance - Floor(distance / TWO_PI) * TWO_PI, 0f, TWO_PI);

			return radDistance > PI ? radDistance - TWO_PI : radDistance;
		}

		// Faster then Mathf.MoveTowardsAngle by ~50%
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float SmoothStepRad(this float current, float target, float step)
		{
			var distance = target - current;
			distance = distance.NormalizeRad();

			var distanceSign = distance.Sign();

			var delta = distance * distanceSign;
			var delta2 = TWO_PI - delta;

			if (delta > delta2)
			{
				distanceSign = -distanceSign;
				delta = delta2;
			}

			return delta > step ? current + step * distanceSign : target;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float NormalizeRad(this float rad)
		{
			rad %= TWO_PI;
			return rad < 0f ? rad + TWO_PI : rad;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string ToInvariantString(this float value)
		{
			return value.ToString(CultureInfo.InvariantCulture);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string FromToString(this float from, float to)
		{
			return $"{from.ToInvariantString()}-{to.ToInvariantString()}";
		}
	}
}
