using System;
using System.Runtime.CompilerServices;

namespace Content
{
	public abstract partial class BaseContentEntry<T> : IContentEntry<T>
	{
#if !CLIENT
		// ReSharper disable once InconsistentNaming
		protected T value;
		public ref readonly T Value => ref value;
		public void SetValue(in T newValue) => value = newValue;
#endif

		public Type ValueType => typeof(T);
		public virtual object Context => null;
		public virtual bool IsValid() => true;

		public virtual bool IsUnique() => false;

		public BaseContentEntry(in T value)
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
		public const string DEFAULT_SINGLE_ID = "Single";

		/// <summary>
		/// <see cref="ContentEntry{T}.guid"/>
		/// </summary>
		public const string GUID_FIELD_NAME = "guid";

		/// <summary>
		/// <see cref="BaseContentEntry{T}.value"/>
		/// </summary>
		public const string VALUE_FIELD_NAME = "value";

		public Type ValueType { get; }
		public object Context { get; }
		public void Register();
		public void Unregister();
		public bool IsUnique();
	}
}
