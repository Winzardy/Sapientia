using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public struct PtrOffset
	{
		public readonly int byteOffset;
		public readonly bool isValid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PtrOffset(int byteOffset)
		{
			this.byteOffset = byteOffset;
			this.isValid = true;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset operator +(PtrOffset a, PtrOffset b)
		{
			return new PtrOffset(a.byteOffset + b.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset operator +(PtrOffset offset, int byteOffset)
		{
			return new PtrOffset(offset.byteOffset + byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset operator -(PtrOffset a, PtrOffset b)
		{
			return new PtrOffset(a.byteOffset - b.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset operator ++(PtrOffset offset)
		{
			return new PtrOffset(offset.byteOffset + 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset operator --(PtrOffset offset)
		{
			return new PtrOffset(offset.byteOffset - 1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset operator -(PtrOffset offset)
		{
			return new PtrOffset(-offset.byteOffset);
		}
	}

	public struct PtrOffset<T>
		where T : unmanaged
	{
		public readonly int byteOffset;
		public readonly bool isValid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PtrOffset(int byteOffset)
		{
			this.byteOffset = byteOffset;
			this.isValid = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PtrOffset<T> Offset(int bytes)
		{
			return new PtrOffset<T>(byteOffset + bytes);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator -(PtrOffset<T> a, PtrOffset<T> b)
		{
			return new PtrOffset<T>(a.byteOffset - b.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator -(PtrOffset a, PtrOffset<T> b)
		{
			return new PtrOffset<T>(a.byteOffset - b.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator -(PtrOffset<T> a, PtrOffset b)
		{
			return new PtrOffset<T>(a.byteOffset - b.byteOffset);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator +(PtrOffset<T> a, PtrOffset<T> b)
		{
			return new PtrOffset<T>(a.byteOffset + b.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator +(PtrOffset a, PtrOffset<T> b)
		{
			return new PtrOffset<T>(a.byteOffset + b.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator +(PtrOffset<T> a, PtrOffset b)
		{
			return new PtrOffset<T>(a.byteOffset + b.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator +(PtrOffset<T> offset, int count)
		{
			return new PtrOffset<T>(offset.byteOffset + count * TSize<T>.size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator ++(PtrOffset<T> offset)
		{
			return new PtrOffset<T>(offset.byteOffset + TSize<T>.size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator --(PtrOffset<T> offset)
		{
			return new PtrOffset<T>(offset.byteOffset - TSize<T>.size);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static PtrOffset<T> operator -(PtrOffset<T> offset)
		{
			return new PtrOffset<T>(-offset.byteOffset);
		}
	}
}
