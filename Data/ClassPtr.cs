using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Sapientia.Extensions;

namespace Sapientia.Data
{
#if UNITY_5_3_OR_NEWER
	using NativeDisableUnsafePtrRestriction = Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestrictionAttribute;
#else
	using NativeDisableUnsafePtrRestriction = PlaceholderAttribute;
#endif

	public unsafe struct ClassPtr : IEquatable<ClassPtr>, IDisposable
	{
		[NativeDisableUnsafePtrRestriction]
		private IntPtr _ptr;
		[NativeDisableUnsafePtrRestriction]
		private GCHandle _gcHandle;

		public bool IsValid => _ptr.ToPointer() != null;

		public IntPtr Ptr => _ptr;
		public GCHandle GcHandle => _gcHandle;

		public ClassPtr(IntPtr ptr, GCHandle gcHandle)
		{
			_ptr = ptr;
			_gcHandle = gcHandle;
		}

		public T Cast<T>()
			where T: class
		{
			if (!_gcHandle.IsAllocated)
				return null!;
			return UnsafeExt.As<object, T>(_gcHandle.Target);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ClassPtr Create<T>(T data)
			where T: class
		{
			var gcHandle = data == null! ? default : GCHandle.Alloc(data);
			var ptr = GCHandle.ToIntPtr(gcHandle);
			return new ClassPtr
			{
				_gcHandle = gcHandle,
				_ptr = ptr,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (_gcHandle.IsAllocated)
			{
				_gcHandle.Free();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ClassPtr other)
		{
			return other._ptr == _ptr;
		}
	}

	public unsafe struct ClassPtr<T> : IEquatable<ClassPtr<T>>, IDisposable
		where T : class
	{
		[NativeDisableUnsafePtrRestriction]
		private IntPtr _ptr;
		[NativeDisableUnsafePtrRestriction]
		private GCHandle _gcHandle;

		public bool IsValid => _ptr.ToPointer() != null;
		public IntPtr Ptr => _ptr;
		public GCHandle GcHandle => _gcHandle;

		public ClassPtr(IntPtr ptr, GCHandle gcHandle)
		{
			_ptr = ptr;
			_gcHandle = gcHandle;
		}

		public readonly T Value()
		{
			if (!_gcHandle.IsAllocated)
				return null!;
			return UnsafeExt.As<object, T>(_gcHandle.Target);
		}

		public static ClassPtr<T> Create<T1>(ClassPtr<T1> data)
			where T1: class, T
		{
			return new ClassPtr<T>
			{
				_gcHandle = data._gcHandle,
				_ptr = data._ptr,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ClassPtr(T data)
		{
			_gcHandle = data == null! ? default : GCHandle.Alloc(data);
			_ptr = GCHandle.ToIntPtr(_gcHandle);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			if (_gcHandle.IsAllocated)
			{
				_gcHandle.Free();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Equals(ClassPtr<T> other)
		{
			return other._ptr == _ptr;
		}

		public static implicit operator ClassPtr(ClassPtr<T> value)
		{
			return new ClassPtr(value._ptr, value._gcHandle);
		}

		public static implicit operator ClassPtr<T>(ClassPtr value)
		{
			return new ClassPtr<T>(value.Ptr, value.GcHandle);
		}
	}
}
