using System;
using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Sapientia.Extensions
{
	public static unsafe class UnsafeExtensions
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static unsafe T1 As<T, T1>(this ref T value) where T : struct where T1 : struct
		{
#if UNITY_5_3_OR_NEWER
			return UnsafeUtility.As<T, T1>(ref value);
#else
			return Unsafe.As<T, T1>(ref value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* AsPointer<T>(this ref T value) where T : struct
		{
#if UNITY_5_3_OR_NEWER
			return UnsafeUtility.AddressOf(ref value);
#else
			return Unsafe.AsPointer(ref value);
#endif
		}

		public static bool IsEquals<T>(this ref T a, ref T b) where T : struct
		{
#if UNITY_5_3_OR_NEWER
			var size = UnsafeUtility.SizeOf<T>();
			return UnsafeUtility.MemCmp(a.AsPointer(), b.AsPointer(), size) == 0;
#else
			var spanA = new ReadOnlySpan<T>(Unsafe.AsPointer(ref a), 1);
			var spanB = new ReadOnlySpan<T>(Unsafe.AsPointer(ref b), 1);
			return spanA.SequenceEqual(spanB);
#endif
		}

		public static bool IsDefault<T>(this ref T value) where T : struct
		{
			var defaultValue = default(T);
#if UNITY_5_3_OR_NEWER
			var size = UnsafeUtility.SizeOf<T>();
			return UnsafeUtility.MemCmp(value.AsPointer(), defaultValue.AsPointer(), size) == 0;
#else
			var spanA = new ReadOnlySpan<T>(Unsafe.AsPointer(ref value), 1);
			var spanB = new ReadOnlySpan<T>(Unsafe.AsPointer(ref defaultValue), 1);
			return spanA.SequenceEqual(spanB);
#endif
		}
	}
}