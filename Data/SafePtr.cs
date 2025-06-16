using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sapientia.Data
{
	[StructLayout(LayoutKind.Sequential)]
#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	public readonly unsafe struct SafePtr
	{
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public readonly byte* ptr;
#if DEBUG
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public readonly byte* lowBound;
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public readonly byte* hiBound;

		public byte* HiBound => hiBound;
		public byte* LowBound => lowBound;
#else
		public byte* HiBound => this.ptr;
		public byte* LowBound => this.ptr;
#endif

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ptr != null;
		}

		public ref byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
#if DEBUG
				var result = (ptr + index);
				E.ASSERT((result - lowBound >= 0) && (hiBound - result > 0));
#endif
				return ref ptr[index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr(void* ptr)
		{
			this.ptr = (byte*)ptr;
#if DEBUG
			lowBound = null;
			hiBound = null;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr(void* ptr, int size)
		{
			this.ptr = (byte*)ptr;
#if DEBUG
			lowBound = this.ptr;
			hiBound = this.ptr + size;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr(void* ptr, byte* hiBound)
		{
			this.ptr = (byte*)ptr;
#if DEBUG
			lowBound = this.ptr;
			this.hiBound = hiBound;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal SafePtr(void* ptr, byte* lowBound, byte* hiBound)
		{
			this.ptr = (byte*)ptr;
#if DEBUG
			this.lowBound = lowBound;
			this.hiBound = hiBound;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator SafePtr(void* ptr)
		{
			return new SafePtr(ptr, 0);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator byte*(SafePtr safePtr)
		{
			return safePtr.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Value<T>() where T : unmanaged
		{
			return ref *(T*)ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<U> Cast<U>() where U : unmanaged
		{
#if DEBUG
			return new SafePtr<U>((U*)ptr, lowBound, hiBound);
#else
			return new SafePtr<U>((U*)this.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidLength(int length)
		{
#if DEBUG
			var newPtr = ptr + length;
			return newPtr >= lowBound && newPtr <= hiBound;
#else
			return true;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr operator +(SafePtr safePtr, int index)
		{
			var newPtr = safePtr.ptr + index;
#if DEBUG
			E.ASSERT((newPtr - safePtr.lowBound >= 0) && (safePtr.hiBound - newPtr > 0));

			return new SafePtr(newPtr, safePtr.lowBound, safePtr.hiBound);
#else
			return new SafePtr(newPtr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr operator -(SafePtr safePtr, int index)
		{
			var newPtr = safePtr.ptr - index;
#if DEBUG
			E.ASSERT((newPtr - safePtr.lowBound >= 0) && (safePtr.hiBound - newPtr > 0));

			return new SafePtr(newPtr, safePtr.lowBound, safePtr.hiBound);
#else
			return new SafePtr(newPtr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SafePtr left, SafePtr right)
		{
#if DEBUG
			if (left.lowBound != right.lowBound || left.hiBound != right.hiBound)
				return false;
#endif
			return left.ptr == right.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SafePtr left, SafePtr right)
		{
#if DEBUG
			if (left.lowBound != right.lowBound || left.hiBound != right.hiBound)
				return true;
#endif
			return left.ptr != right.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return (int)ptr;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
#if UNITY_5_3_OR_NEWER
	[Unity.Burst.BurstCompile]
#endif
	public readonly unsafe struct SafePtr<T> where T: unmanaged
	{
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public readonly T* ptr;
#if DEBUG
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public readonly byte* lowBound;
#if UNITY_5_3_OR_NEWER
		[Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
		public readonly byte* hiBound;

		public byte* HiBound => hiBound;
		public byte* LowBound => lowBound;
#else
		public byte* HiBound => (byte*)ptr;
		public byte* LowBound => (byte*)ptr;
#endif

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ptr != null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr(T* ptr)
		{
			this.ptr = ptr;
#if DEBUG
			lowBound = null;
			hiBound = null;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr(void* ptr, int length)
		{
			this.ptr = (T*)ptr;
#if DEBUG
			lowBound = (byte*)ptr;
			hiBound = (byte*)(this.ptr + length);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr(T* ptr, int length)
		{
			this.ptr = ptr;
#if DEBUG
			lowBound = (byte*)ptr;
			hiBound = (byte*)(this.ptr + length);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal SafePtr(T* ptr, byte* lowBound, byte* hiBound)
		{
			this.ptr = ptr;
#if DEBUG
			this.lowBound = lowBound;
			this.hiBound = hiBound;
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Value()
		{
			return ref *ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<U> Cast<U>() where U : unmanaged
		{
#if DEBUG
			return new SafePtr<U>((U*)ptr, lowBound, hiBound);
#else
			return new SafePtr<U>((U*)this.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsLengthInRange(int length)
		{
#if DEBUG
			var newPtr = ptr + length;
			return newPtr >= lowBound && newPtr <= hiBound;
#else
			return true;
#endif
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
#if DEBUG
				var result = (byte*)(ptr + index);
				E.ASSERT((result - lowBound >= 0) && (hiBound - result > 0));
#endif
				return ref ptr[index];
			}
		}

		public SafePtr<T> Slice(int index, int length = 1)
		{
#if DEBUG
			var result = (byte*)(ptr + index);
			E.ASSERT((result - lowBound >= 0) && (hiBound - (result + length) >= 0));
#endif
			return new SafePtr<T>(ptr + index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SafePtr(SafePtr<T> safePtr)
		{
#if DEBUG
			return new SafePtr(safePtr.ptr, safePtr.lowBound, safePtr.hiBound);
#else
			return new SafePtr(safePtr.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SafePtr<T>(SafePtr safePtr)
		{
#if DEBUG
			return new SafePtr<T>((T*)safePtr.ptr, safePtr.lowBound, safePtr.hiBound);
#else
			return new SafePtr<T>((T*)safePtr.ptr);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator T*(SafePtr<T> safePtr)
		{
			return safePtr.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator ++(SafePtr<T> safePtr)
		{
			return safePtr + 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator --(SafePtr<T> safePtr)
		{
			return safePtr - 1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator +(SafePtr<T> safePtr, int index)
		{
			var newPtr = safePtr.ptr + index;
#if DEBUG
			E.ASSERT(((byte*)newPtr - safePtr.lowBound >= 0) && (safePtr.hiBound - (byte*)newPtr > 0));
			return new SafePtr<T>(newPtr, safePtr.lowBound, safePtr.hiBound);
#else
			return new SafePtr<T>(safePtr.ptr + index);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator -(SafePtr<T> safePtr, int index)
		{
			var newPtr = safePtr.ptr - index;
#if DEBUG
			E.ASSERT(((byte*)newPtr - safePtr.lowBound >= 0) && (safePtr.hiBound - (byte*)newPtr > 0));
			return new SafePtr<T>(newPtr, safePtr.lowBound, safePtr.hiBound);
#else
			return new SafePtr<T>(safePtr.ptr - index);
#endif
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SafePtr<T> left, SafePtr<T> right)
		{
#if DEBUG
			if (left.lowBound != right.lowBound || left.hiBound != right.hiBound)
				return false;
#endif
			return left.ptr == right.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SafePtr<T> left, SafePtr<T> right)
		{
#if DEBUG
			if (left.lowBound != right.lowBound || left.hiBound != right.hiBound)
				return true;
#endif
			return left.ptr != right.ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			return (int)ptr;
		}
	}
}
