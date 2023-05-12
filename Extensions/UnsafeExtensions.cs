using System.Runtime.CompilerServices;
#if UNITY_5_3_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Sapientia.Extensions
{
	public static unsafe class UnsafeExtensions
	{
		private const int INT_SIZE = sizeof(int);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void* AsRefAsPointer<T>(in T value)
		{
#if UNITY_5_3_OR_NEWER
			return UnsafeUtility.AsPointer(ref UnsafeUtility.AsRef(value));
#else
			return Unsafe.AsPointer(ref Unsafe.AsRef(value));
#endif
		}

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
		public static unsafe T1 As<T, T1>(this T value)
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void* AsPointer<T>(this T value)
		{
#if UNITY_5_3_OR_NEWER
			return UnsafeUtility.AddressOf(ref value);
#else
			return Unsafe.AsPointer(ref value);
#endif
		}

		public static bool IsEquals<T>(this T a, in T b)
		{
#if UNITY_5_3_OR_NEWER
			var size = UnsafeUtility.SizeOf<T>();
#else
			var size = Unsafe.SizeOf<T>();
#endif

			var intPtrA = (int*)AsRefAsPointer(a);
			var intPtrB = (int*)AsRefAsPointer(b);

			var bytes = size % INT_SIZE;
			var ints = (size - bytes) / INT_SIZE;
			for (var i = 0; i < ints; i++)
			{
				if (intPtrA[i] != intPtrB[i])
					return false;
			}
			var bytePtrA = (byte*)intPtrA;
			var bytePtrB = (byte*)intPtrB;
			for (var i = size - bytes; i < size; i++)
			{
				if (bytePtrA[i] != bytePtrB[i])
					return false;
			}

			return true;
		}

		public static bool IsDefault<T>(this ref T value) where T : unmanaged
		{
			var size = sizeof(T);

			var bytes = size % sizeof(int);
			var ints = (size - bytes) / sizeof(int);

			var intPtr = (int*)AsPointer(ref value);
			for (var i = 0; i < ints; i++)
			{
				if (intPtr[i] != default)
					return false;
			}
			var bytePtr = (byte*)intPtr;
			for (var i = size - bytes; i < size; i++)
			{
				if (bytePtr[i] != default)
					return false;
			}

			return true;
		}
	}
}