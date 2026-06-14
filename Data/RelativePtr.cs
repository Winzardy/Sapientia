using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Data
{
	public struct RelativePtr
	{
		public readonly int byteOffset;
		public readonly bool isValid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RelativePtr(int byteOffset)
		{
			this.byteOffset = byteOffset;
			this.isValid = true;
		}

		public void SetPtr(SafePtr dataPtr)
		{
			this = (RelativePtr)(dataPtr - (SafePtr)this.AsSafePtr());
		}

		public unsafe SafePtr GetPtr()
		{
			E.ASSERT(isValid);
			var fieldPtr = (SafePtr)this.AsSafePtr();
			var basePtr = new SafePtr(fieldPtr.ptr + byteOffset);
			return basePtr;
		}

		public unsafe SafePtr<T> GetPtr<T>() where T: unmanaged
		{
			E.ASSERT(isValid);
			var fieldPtr = (SafePtr)this.AsSafePtr();
			var basePtr = new SafePtr<T>(fieldPtr.ptr + byteOffset, 1);
			return basePtr;
		}

		public unsafe SafePtr<T> GetPtr<T>(PtrOffset<T> offset) where T: unmanaged
		{
			E.ASSERT(isValid);
			var fieldPtr = (SafePtr)this.AsSafePtr();
			var basePtr = new SafePtr<T>(fieldPtr.ptr + byteOffset + offset.byteOffset, 1);
			return basePtr;
		}

		public ref T GetValue<T>() where T: unmanaged
		{
			return ref GetPtr<T>().Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator RelativePtr(PtrOffset offset)
		{
			return new RelativePtr(offset.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator PtrOffset(RelativePtr relativePtr)
		{
			return new PtrOffset(relativePtr.byteOffset);
		}
	}

	public struct RelativePtr<T>
		where T : unmanaged
	{
		public readonly int byteOffset;
		public readonly bool isValid;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public RelativePtr(int byteOffset)
		{
			this.byteOffset = byteOffset;
			this.isValid = true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public PtrOffset<T> Offset(int bytes)
		{
			return new PtrOffset<T>(byteOffset + bytes);
		}

		public void SetPtr(SafePtr<T> dataPtr)
		{
			this = (RelativePtr<T>)(dataPtr - (SafePtr)this.AsSafePtr());
		}

		public unsafe SafePtr<T> GetPtr()
		{
			E.ASSERT(isValid);
			var fieldPtr = (SafePtr)this.AsSafePtr();
			var basePtr = new SafePtr<T>(fieldPtr.ptr + byteOffset, 1);
			return basePtr;
		}

		public ref T GetValue()
		{
			return ref GetPtr().Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator RelativePtr<T>(RelativePtr relativePtr)
		{
			return new RelativePtr<T>(relativePtr.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator RelativePtr(RelativePtr<T> relativePtr)
		{
			return new RelativePtr(relativePtr.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator RelativePtr<T>(PtrOffset offset)
		{
			return new RelativePtr<T>(offset.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator RelativePtr<T>(PtrOffset<T> offset)
		{
			return new RelativePtr<T>(offset.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator PtrOffset(RelativePtr<T> relativePtr)
		{
			return new PtrOffset(relativePtr.byteOffset);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator PtrOffset<T>(RelativePtr<T> relativePtr)
		{
			return new PtrOffset<T>(relativePtr.byteOffset);
		}
	}
}
