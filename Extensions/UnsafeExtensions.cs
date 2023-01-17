using Unity.Collections.LowLevel.Unsafe;

namespace Sapientia.Extensions
{
	public static unsafe class UnsafeExtensions
	{
		public static bool IsEquals<T>(this ref T a, ref T b) where T : unmanaged
		{
			var size = sizeof(T);

			var bytes = size % sizeof(int);
			var ints = (size - bytes) / sizeof(int);

			var intPtrA = (int*)UnsafeUtility.AddressOf(ref a);
			var intPtrB = (int*)UnsafeUtility.AddressOf(ref b);
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

			var intPtr = (int*)UnsafeUtility.AddressOf(ref value);
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