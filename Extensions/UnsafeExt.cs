using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if UNITY_5_3_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Sapientia.Extensions
{
	public struct TSize<T> where T : struct
	{
		public static readonly int size = UnsafeExt.SizeOf<T>();
		public static readonly uint uSize = (uint)UnsafeExt.SizeOf<T>();
	}

	public struct TAlign<T> where T : struct
	{
		public static readonly int align = UnsafeExt.AlignOf<T>();
	}

	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#cdd8c9f157a24ed3951f9de198b67b59
	/// </summary>
	public static unsafe class UnsafeExt
	{
		// Copy of UnsafeUtility.AlignOfHelper<T> for AlignOf<T>()
		private struct AlignOfHelper<T> where T : struct
		{
			public byte dummy;
			public T data;
		}

		// Copy of UnsafeUtility.SizeOf<T>()
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int SizeOf<T>() where T : struct
		{
#if UNITY_5_3_OR_NEWER
			return UnsafeUtility.SizeOf<T>();
#else
			return Unsafe.SizeOf<T>();
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int AlignOf<T>() where T : struct
		{
#if UNITY_5_3_OR_NEWER
			// Copy of UnsafeUtility.AlignOf<T>()
			return UnsafeUtility.SizeOf<AlignOfHelper<T>>() - UnsafeUtility.SizeOf<T>();
#else
			// Copy of UnsafeUtility.AlignOf<T>()
			return Unsafe.SizeOf<AlignOfHelper<T>>() - Unsafe.SizeOf<T>();
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AsRef<T>(void* ptr) where T : struct
		{
#if UNITY_5_3_OR_NEWER
			return ref UnsafeUtility.AsRef<T>(ptr);
#else
			return ref Unsafe.AsRef<T>(ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T1 As<T, T1>(this ref T value) where T : struct where T1 : struct
		{
#if UNITY_5_3_OR_NEWER
			return ref UnsafeUtility.As<T, T1>(ref value);
#else
			return ref Unsafe.As<T, T1>(ref value);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Offset(byte* from, byte* to)
		{
			return to - from;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Offset<T>(this ref T from, byte* to) where T: unmanaged
		{
			return to - (byte*)from.AsPointer();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Offset<T, T1>(this ref T from, ref T1 to)
			where T: unmanaged
			where T1: unmanaged
		{
			return (byte*)to.AsPointer() - (byte*)from.AsPointer();
		}
	}
}
