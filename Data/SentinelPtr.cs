using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Submodules.Sapientia.Safety;

namespace Sapientia.Data
{
	/// <summary>
	/// SentinelPtr — указатель с проверкой валидности и автоматическим контролем времени жизни через DisposeSentinel.
	/// Обеспечивает проверку валидности и выбрасывает исключение при обращении к невалидной памяти.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SentinelPtr : IDisposable
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
		public SentinelPtr(SafePtr ptr)
		{
			_ptr = ptr;
			_disposeSentinel = DisposeSentinel.Create();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SentinelPtr(SafePtr ptr, DisposeSentinel disposeSentinel)
		{
			_ptr = ptr;
			_disposeSentinel = disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr Create(SafePtr ptr)
		{
			return new SentinelPtr(ptr);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr Create<T>(SafePtr ptr)
		{
			return new SentinelPtr(ptr, DisposeSentinel.Create<T>());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		[Conditional("DEBUG")]
		private void CheckNullRef()
		{
			if (!_disposeSentinel.IsValid())
				throw new NullReferenceException("SafetyHandle is not valid");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator byte*(SentinelPtr safePtr)
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
		public SentinelPtr<U> Cast<U>() where U : unmanaged
		{
			CheckNullRef();
			return new SentinelPtr<U>(_ptr, _disposeSentinel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool IsValidLength(int length)
		{
			CheckNullRef();
			return _ptr.IsValidLength(length);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr operator +(SentinelPtr sentinelPtr, int index)
		{
			sentinelPtr.CheckNullRef();
			sentinelPtr._ptr += index;
			return sentinelPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr operator -(SentinelPtr sentinelPtr, int index)
		{
			sentinelPtr.CheckNullRef();
			sentinelPtr._ptr -= index;
			return sentinelPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SentinelPtr left, SentinelPtr right)
		{
			return left._ptr == right._ptr && left._disposeSentinel == right._disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SentinelPtr left, SentinelPtr right)
		{
			return left._ptr != right._ptr || left._disposeSentinel != right._disposeSentinel;
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

		/// <summary>
		/// Сбрасывает текущий DisposeSentinel, освобождая старый и создавая новый для контроля времени жизни указателя.
		/// Используется для обновления механизма отслеживания валидности указателя после повторного использования или переназначения.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetDisposeSentinel()
		{
			_disposeSentinel.Dispose();
			_disposeSentinel = DisposeSentinel.Create();
		}
	}

	/// <summary>
	/// SentinelPtr&lt;T&gt; — указатель с проверкой валидности и автоматическим контролем времени жизни через DisposeSentinel.
	/// Обеспечивает проверку валидности и выбрасывает исключение при обращении к невалидной памяти.
	/// </summary>
	[StructLayout(LayoutKind.Sequential)]
	public unsafe struct SentinelPtr<T> : IDisposable where T: unmanaged
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
		public SentinelPtr(SafePtr<T> ptr)
		{
			_ptr = ptr;
			_disposeSentinel = DisposeSentinel.Create<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SentinelPtr(SafePtr<T> ptr, DisposeSentinel disposeSentinel)
		{
			_ptr = ptr;
			_disposeSentinel = disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr<T> Create(SafePtr<T> ptr)
		{
			return new SentinelPtr<T>(ptr);
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
		public SentinelPtr<U> Cast<U>() where U : unmanaged
		{
			CheckNullRef();
			return new SentinelPtr<U>(_ptr.Cast<U>(), _disposeSentinel);
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
		public static implicit operator SentinelPtr(SentinelPtr<T> sentinelPtr)
		{
			return new SentinelPtr(sentinelPtr._ptr, sentinelPtr._disposeSentinel);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator SentinelPtr<T>(SentinelPtr sentinelPtr)
		{
			return sentinelPtr.Cast<T>();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator T*(SentinelPtr<T> sentinelPtr)
		{
			sentinelPtr.CheckNullRef();
			return (T*)sentinelPtr._ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr<T> operator ++(SentinelPtr<T> sentinelPtr)
		{
			sentinelPtr.CheckNullRef();
			sentinelPtr._ptr++;
			return sentinelPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr<T> operator --(SentinelPtr<T> sentinelPtr)
		{
			sentinelPtr.CheckNullRef();
			sentinelPtr._ptr--;
			return sentinelPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr<T> operator +(SentinelPtr<T> sentinelPtr, int index)
		{
			sentinelPtr.CheckNullRef();
			sentinelPtr._ptr += index;
			return sentinelPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static SentinelPtr<T> operator -(SentinelPtr<T> sentinelPtr, int index)
		{
			sentinelPtr.CheckNullRef();
			sentinelPtr._ptr -= index;
			return sentinelPtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator ==(SentinelPtr<T> left, SentinelPtr<T> right)
		{
			return left._ptr == right._ptr && left._disposeSentinel == right._disposeSentinel;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool operator !=(SentinelPtr<T> left, SentinelPtr<T> right)
		{
			return left._ptr != right._ptr || left._disposeSentinel != right._disposeSentinel;
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

		/// <summary>
		/// Сбрасывает текущий DisposeSentinel, освобождая старый и создавая новый для контроля времени жизни указателя.
		/// Используется для обновления механизма отслеживания валидности указателя после повторного использования или переназначения.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void ResetDisposeSentinel()
		{
			_disposeSentinel.Dispose();
			_disposeSentinel = DisposeSentinel.Create<T>();
		}
	}
}
