using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Submodules.Sapientia.Safety;

namespace Sapientia.Data
{
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct NullablePtr : IDisposable
	{
		private SafePtr _ptr;
		private DisposeSentinel _disposeSentinel;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ptr.IsValid && _disposeSentinel.IsValid() ;
		}

		public ref byte this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckNullRef();
				return ref _ptr[index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NullablePtr(SafePtr ptr)
		{
			_ptr = ptr;
			_disposeSentinel = DisposeSentinel.Create();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NullablePtr(SafePtr ptr, DisposeSentinel disposeSentinel)
		{
			_ptr = ptr;
			_disposeSentinel = disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr Create(SafePtr ptr)
		{
			return new NullablePtr(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr Create<T>(SafePtr ptr)
		{
			return new NullablePtr(ptr, DisposeSentinel.Create<T>());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("DEBUG")]
		private void CheckNullRef()
		{
			if (!_disposeSentinel.IsValid())
				throw new NullReferenceException("SafetyHandle is not valid");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator byte*(NullablePtr safePtr)
		{
			safePtr.CheckNullRef();
			return (byte*)safePtr._ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr GetPtr()
		{
			CheckNullRef();
			return _ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Value<T>() where T : unmanaged
		{
			CheckNullRef();
			return ref _ptr.Value<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NullablePtr<U> Cast<U>() where U : unmanaged
		{
			CheckNullRef();
			return new NullablePtr<U>(_ptr, _disposeSentinel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidLength(int length)
		{
			CheckNullRef();
			return _ptr.IsValidLength(length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr operator +(NullablePtr nullablePtr, int index)
		{
			nullablePtr.CheckNullRef();
			nullablePtr._ptr += index;
			return nullablePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr operator -(NullablePtr nullablePtr, int index)
		{
			nullablePtr.CheckNullRef();
			nullablePtr._ptr -= index;
			return nullablePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(NullablePtr left, NullablePtr right)
		{
			return left._ptr == right._ptr && left._disposeSentinel == right._disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(NullablePtr left, NullablePtr right)
		{
			return left._ptr != right._ptr && left._disposeSentinel != right._disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			CheckNullRef();
			return HashCode.Combine(_ptr.GetHashCode(), _disposeSentinel.GetHashCode());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_disposeSentinel.Dispose();
			_disposeSentinel = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetDisposeSentinel()
		{
			_disposeSentinel.Dispose();
			_disposeSentinel = DisposeSentinel.Create();
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct NullablePtr<T> : IDisposable where T: unmanaged
	{
		private SafePtr<T> _ptr;
		private DisposeSentinel _disposeSentinel;

		public bool IsValid
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _ptr.IsValid && _disposeSentinel.IsValid() ;
		}

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckNullRef();
				return ref _ptr[index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NullablePtr(SafePtr<T> ptr)
		{
			_ptr = ptr;
			_disposeSentinel = DisposeSentinel.Create<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NullablePtr(SafePtr<T> ptr, DisposeSentinel disposeSentinel)
		{
			_ptr = ptr;
			_disposeSentinel = disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr<T> Create(SafePtr<T> ptr)
		{
			return new NullablePtr<T>(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("DEBUG")]
		private void CheckNullRef()
		{
			if (!_disposeSentinel.IsValid())
				throw new NullReferenceException("SafetyHandle is not valid");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Value()
		{
			CheckNullRef();
			return ref _ptr.Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetPtr()
		{
			CheckNullRef();
			return _ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public NullablePtr<U> Cast<U>() where U : unmanaged
		{
			CheckNullRef();
			return new NullablePtr<U>(_ptr.Cast<U>(), _disposeSentinel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsLengthInRange(int length)
		{
			CheckNullRef();
			return _ptr.IsLengthInRange(length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> Slice(int index, int length = 1)
		{
			CheckNullRef();
			return _ptr.Slice(index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> GetSpan(int index, int length = 1)
		{
			CheckNullRef();
			return _ptr.GetSpan(index, length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator NullablePtr(NullablePtr<T> nullablePtr)
		{
			return new NullablePtr(nullablePtr._ptr, nullablePtr._disposeSentinel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator NullablePtr<T>(NullablePtr nullablePtr)
		{
			return nullablePtr.Cast<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator T*(NullablePtr<T> nullablePtr)
		{
			nullablePtr.CheckNullRef();
			return (T*)nullablePtr._ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr<T> operator ++(NullablePtr<T> nullablePtr)
		{
			nullablePtr.CheckNullRef();
			nullablePtr._ptr++;
			return nullablePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr<T> operator --(NullablePtr<T> nullablePtr)
		{
			nullablePtr.CheckNullRef();
			nullablePtr._ptr--;
			return nullablePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr<T> operator +(NullablePtr<T> nullablePtr, int index)
		{
			nullablePtr.CheckNullRef();
			nullablePtr._ptr += index;
			return nullablePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static NullablePtr<T> operator -(NullablePtr<T> nullablePtr, int index)
		{
			nullablePtr.CheckNullRef();
			nullablePtr._ptr -= index;
			return nullablePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(NullablePtr<T> left, NullablePtr<T> right)
		{
			return left._ptr == right._ptr && left._disposeSentinel == right._disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(NullablePtr<T> left, NullablePtr<T> right)
		{
			return left._ptr != right._ptr && left._disposeSentinel != right._disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public override int GetHashCode()
		{
			CheckNullRef();
			return HashCode.Combine(_ptr.GetHashCode(), _disposeSentinel.GetHashCode());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			_disposeSentinel.Dispose();
			_disposeSentinel = default;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetDisposeSentinel()
		{
			_disposeSentinel.Dispose();
			_disposeSentinel = DisposeSentinel.Create<T>();
		}
	}
}
