using System;
using Sapientia.Extensions;

namespace Submodules.Sapientia.Data
{
#if UNITY_EDITOR
	[Sirenix.OdinInspector.InlineProperty]
#endif
	[Serializable]
	public struct EnumSerializableContainer<TEnum>
#if UNITY_EDITOR
		: UnityEngine.ISerializationCallbackReceiver
#endif
		where TEnum : unmanaged, Enum
	{
		/// <summary>
		/// Поле предназначено для правильной сериализации на случай, если список статов будет изменён.
		/// </summary>
#if UNITY_EDITOR
		[UnityEngine.HideInInspector]
		public string statTypeName;
#endif

		#if UNITY_EDITOR
		[Sirenix.OdinInspector.HideLabel]
		[Sirenix.OdinInspector.EnumPaging]
		#endif
		public TEnum value;

		public static implicit operator TEnum(EnumSerializableContainer<TEnum> container)
		{
			return container.value;
		}

		public static implicit operator EnumSerializableContainer<TEnum>(TEnum value)
		{
			return new EnumSerializableContainer<TEnum>{ value = value };
		}

#if UNITY_EDITOR
		void UnityEngine.ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			statTypeName = value.ToString();
		}

		void UnityEngine.ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (statTypeName.IsNullOrEmpty() || !Enum.TryParse<TEnum>(statTypeName, out var enumValue))
			{
				statTypeName = value.ToString();
			}
			else if (!value.Equals(enumValue))
			{
				value = enumValue;
			}
		}
#endif
	}
}
