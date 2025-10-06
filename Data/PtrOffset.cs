using System.Runtime.CompilerServices;

namespace Sapientia.Data
{
	public struct PtrOffset
	{
		public readonly int byteOffset;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PtrOffset(int byteOffset)
		{
			this.byteOffset = byteOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr operator -(SafePtr safePtr, PtrOffset offset)
		{
			return safePtr - offset.byteOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr operator +(SafePtr safePtr, PtrOffset offset)
		{
			return safePtr + offset.byteOffset;
		}
	}

	public struct PtrOffset<T>
		where T : unmanaged
	{
		public readonly int byteOffset;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PtrOffset(int byteOffset)
		{
			this.byteOffset = byteOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator -(SafePtr safePtr, PtrOffset<T> offset)
		{
			return safePtr - offset.byteOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator +(SafePtr safePtr, PtrOffset<T> offset)
		{
			return safePtr + offset.byteOffset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator -(SafePtr<T> safePtr, PtrOffset<T> offset)
		{
			return (SafePtr)safePtr - offset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SafePtr<T> operator +(SafePtr<T> safePtr, PtrOffset<T> offset)
		{
			return (SafePtr)safePtr + offset;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator PtrOffset<T>(PtrOffset  offset)
		{
			return new PtrOffset<T>(offset.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator PtrOffset(PtrOffset<T>  offset)
		{
			return new PtrOffset(offset.byteOffset);
		}
	}
}
