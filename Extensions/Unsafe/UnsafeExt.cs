using System; // Не убирать
using System.Runtime.CompilerServices;
using Sapientia.Data;
#if UNITY_5_3_OR_NEWER
using Unity.Collections.LowLevel.Unsafe;
#endif

namespace Sapientia.Extensions
{
#if !CLIENT
	using UnsafeUtility = System.Runtime.CompilerServices.Unsafe;
#endif

	public struct TSize<T> where T : struct
	{
		public static readonly int size = UnsafeExt.SizeOf<T>();
		public static readonly uint uSize = (uint) UnsafeExt.SizeOf<T>();
	}

	public struct TAlign<T> where T : struct
	{
		public static readonly int align = UnsafeExt.AlignOf<T>();
	}

	public struct TDefaultValue<T> where T : unmanaged
	{
		public static T value = default(T);
	}

	public struct TReadonlyDefaultValue<T>
	{
		public static readonly T value = default!;
	}

	/// <summary>
	/// https://www.notion.so/Extension-b985410501c742dabb3a08ca171a319c?pvs=4#cdd8c9f157a24ed3951f9de198b67b59
	/// </summary>

#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	public static unsafe class UnsafeExt
	{
		// Copy of UnsafeUtility.AlignOfHelper<T> for AlignOf<T>()
		private struct AlignOfHelper<T> where T : struct
		{
			public byte dummy;
			public T data;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T DefaultRef<T>() where T : unmanaged
		{
			return ref TDefaultValue<T>.value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T DefaultRefReadonly<T>()
		{
			return ref TReadonlyDefaultValue<T>.value;
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

#if !UNITY_5_3_OR_NEWER
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T AsRef<T>(in T value) where T : struct
		{
			return ref Unsafe.AsRef(in value);
		}
#endif

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T1 As<T, T1>(ref T value)
		{
#if UNITY_5_3_OR_NEWER
			return ref UnsafeUtility.As<T, T1>(ref value);
#else
			return ref Unsafe.As<T, T1>(ref value);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static T1 As<T, T1>(this T value)
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
		public static Span<T> AsSpan<T>(this ref T value) where T : struct
		{
#if UNITY_5_3_OR_NEWER
			var ptr = UnsafeUtility.AddressOf(ref value);
#else
			var ptr = Unsafe.AsPointer(ref value);
#endif
			return new Span<T>(ptr, 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Span<TSpan> AsSpan<T, TSpan>(this ref T value, int spanLength) where T : struct where TSpan : struct
		{
#if UNITY_5_3_OR_NEWER
			var ptr = UnsafeUtility.AddressOf(ref value);
#else
			var ptr = Unsafe.AsPointer(ref value);
#endif
			return new Span<TSpan>(ptr, spanLength);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> AsSafePtr<T>(this ref T value) where T : unmanaged
		{
			return new SafePtr<T>(value.AsPointer(), 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsEquals<T>(this ref T a, ref T b) where T : struct
		{
#if UNITY_5_3_OR_NEWER
			return UnsafeUtility.MemCmp(a.AsPointer(), b.AsPointer(), TSize<T>.size) == 0;
#else
			var spanA = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref a), TSize<T>.size);
			var spanB = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref b), TSize<T>.size);
			return spanA.SequenceEqual(spanB);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDefault<T>(this ref T value) where T : struct
		{
			var defaultValue = default(T);
#if UNITY_5_3_OR_NEWER
			return UnsafeUtility.MemCmp(value.AsPointer(), defaultValue.AsPointer(), TSize<T>.size) == 0;
#else
			var spanA = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref value), TSize<T>.size);
			var spanB = new ReadOnlySpan<byte>(Unsafe.AsPointer(ref defaultValue), TSize<T>.size);
			return spanA.SequenceEqual(spanB);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Offset(byte* from, byte* to)
		{
			return to - from;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Offset<T>(this ref T from, byte* to) where T : unmanaged
		{
			return to - (byte*) from.AsPointer();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static long Offset<T, T1>(this ref T from, ref T1 to)
			where T : unmanaged
			where T1 : unmanaged
		{
			return (byte*) to.AsPointer() - (byte*) from.AsPointer();
		}

		public static unsafe uint Hash(void* ptr, int bytes)
		{
			// djb2 - Dan Bernstein hash function
			// http://web.archive.org/web/20190508211657/http://www.cse.yorku.ca/~oz/hash.html
			var str = (byte*) ptr;
			var hash = 5381ul;
			while (bytes > 0)
			{
				ulong c = str[--bytes];
				hash = ((hash << 5) + hash) + c;
			}

			return (uint) hash;
		}

		/// <summary>
		/// Быстрая версия <see cref="Enum.HasFlag"/> без бокса и рефлексии.
		/// Проверяет, что все указанные флаги включены.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool HasFlags<T>(this T value, T flag)
			where T : struct, Enum
		{
			if (UnsafeUtility.SizeOf<T>() == 8)
			{
				var v = UnsafeUtility.As<T, ulong>(ref value);
				var f = UnsafeUtility.As<T, ulong>(ref flag);
				return (v & f) == f;
			}

			if (UnsafeUtility.SizeOf<T>() == 4)
			{
				var v = UnsafeUtility.As<T, uint>(ref value);
				var f = UnsafeUtility.As<T, uint>(ref flag);
				return (v & f) == f;
			}

			if (UnsafeUtility.SizeOf<T>() == 2)
			{
				var v = UnsafeUtility.As<T, ushort>(ref value);
				var f = UnsafeUtility.As<T, ushort>(ref flag);
				return (v & f) == f;
			}
			else
			{
				var v = UnsafeUtility.As<T, byte>(ref value);
				var f = UnsafeUtility.As<T, byte>(ref flag);
				return (v & f) == f;
			}
		}
	}

	/// <summary>
	/// Класс расширений для работы с this, дабы избежать конфликтов с другими методами.
	/// </summary>
	public static class UnsafeExtExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T1 As<T, T1>(this ref T value) where T : struct where T1 : struct
		{
#if UNITY_5_3_OR_NEWER
			return ref UnsafeUtility.As<T, T1>(ref value);
#else
			return ref Unsafe.As<T, T1>(ref value);
#endif
		}
	}
}
