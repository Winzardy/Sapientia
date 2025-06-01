using System;
using System.Runtime.CompilerServices;

namespace Content
{
	public abstract partial class BaseContentEntry<T> : IContentEntry<T>
	{
#if !CLIENT
		// ReSharper disable once InconsistentNaming
		protected T value;
		public virtual ref readonly T Value => ref value;
#endif

		public Type ValueType => typeof(T);
		public virtual object Context => null;
		public virtual bool IsValid() => true;

		public virtual bool IsUnique() => false;

		protected BaseContentEntry(in T value)
		{
			this.value = value;
		}

		public void Register()
		{
			OnRegister();
			OnNestedRegister();
		}

		protected virtual void OnRegister()
		{
		}

		public void Unregister()
		{
			OnUnregister();
			OnNestedUnregister();
		}

		protected virtual void OnUnregister()
		{
		}

		#region Operatiors

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(BaseContentEntry<T> entry) => entry.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator bool(BaseContentEntry<T> entry) => !entry.IsEmpty();

		#endregion
	}

	public partial interface IContentEntry<T> : IContentEntry
	{
		public ref readonly T Value { get; }
	}

	public partial interface IContentEntry
	{
		public Type ValueType { get; }

		public void Register();
		public void Unregister();
		public bool IsUnique();

		public object Context { get; }
	}
}
