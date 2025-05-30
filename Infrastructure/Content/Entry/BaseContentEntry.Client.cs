#if CLIENT
using System;
using Sapientia.Extensions.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Content
{
	public partial class BaseContentEntry<T>
	{
		// ReSharper disable once InconsistentNaming
		[SerializeField, FormerlySerializedAs("_value")]
		protected T value;

		public virtual ref readonly T Value => ref ContentEditValue;
		ref T IContentEntry<T>.EditValue => ref ContentEditValue;

		protected ref T ContentEditValue
		{
			get
			{
				if (ValueType.IsSerializeReference())
				{
					_serializeReference ??= new SerializeReference<T>();
					return ref _serializeReference.Value;
				}

				return ref value;
			}
		}

		// ReSharper disable once InconsistentNaming
		// Only inspector
		private T _value
		{
			get
			{
				_serializeReference ??= new SerializeReference<T>();
				return _serializeReference.Value;
			}
			set => _serializeReference.Value = value;
		}

		// TODO: надо очищать YAML от Null значений, потому что тупо место занимает rid: -2...
		// ReSharper disable once InconsistentNaming
		[SerializeField, SerializeReference, ClientOnly]
		private ISerializeReference<T> _serializeReference = null;

		public object RawValue => Value;
	}

	public static class BaseContentEntryExtensions
	{
		public static bool IsSerializeReference(this Type type)
		{
			if (type == null)
				return false;

			type = type.GetCollectionElementType();
			return type.IsPolymorphic() && !type.IsSubclassOf(typeof(Object));
		}
	}

	public partial interface IContentEntry
	{
		/// <summary>
		/// <see cref="BaseContentEntry{T}._serializeReference"/>
		/// </summary>
		public const string UNITY_VALUE_FIELD_NAME = "_serializeReference";

		/// <summary>
		/// <see cref="ContentEntry{T}._value"/>
		/// </summary>
		public const string CUSTOM_VALUE_FIELD_NAME = "_value";

		object RawValue { get; }
	}

	public partial interface IContentEntry<T>
	{
		/// <summary>
		/// Изменение контента доступно только вне runtime!
		/// </summary>
		internal ref T EditValue { get; }
	}

	internal interface ISerializeReference<T>
	{
		internal ref T Value { get; }
	}

	// Использую такой контейнер, в обход сериализации object. У object проблемы с массивами...
	internal class SerializeReference<T> : ISerializeReference<T>
	{
		[SerializeField, SerializeReference, ClientOnly]
		private T _value;

		ref T ISerializeReference<T>.Value => ref _value;
	}
}
#endif
