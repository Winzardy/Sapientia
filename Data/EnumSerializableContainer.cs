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
#if UNITY_EDITOR
		/// <summary>
		/// Поле предназначено для правильной сериализации на случай, если список статов будет изменён.
		/// </summary>
		[UnityEngine.HideInInspector]
		public string statTypeName;
#endif

#if UNITY_EDITOR
		[Sirenix.OdinInspector.EnumPaging]
		[Sirenix.OdinInspector.HideLabel]
#endif
		public TEnum value;

		public EnumSerializableContainer<TEnum1> Convert<TEnum1>()
			where TEnum1 : unmanaged, Enum
		{
			return new EnumSerializableContainer<TEnum1>()
			{
#if UNITY_EDITOR
				statTypeName = statTypeName,
#endif
				value = value.ToEnum<TEnum, TEnum1>(),
			};
		}

		public static implicit operator TEnum(EnumSerializableContainer<TEnum> container)
		{
			return container.value;
		}

		public static implicit operator EnumSerializableContainer<TEnum>(TEnum value)
		{
			return new EnumSerializableContainer<TEnum>{ value = value };
		}

		public EnumSerializableContainer<TEnum1> ConvertTo<TEnum1>()
			where TEnum1 : unmanaged, Enum
		{
			return new EnumSerializableContainer<TEnum1>()
			{
				value = value.ToEnum<TEnum, TEnum1>(),
				statTypeName = statTypeName,
			};
		}

#if UNITY_EDITOR
		public void OnBeforeSerialize()
		{
			statTypeName = value.ToString();
		}

		public void OnAfterDeserialize()
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

	public static class EnumSerializableContainerExt
	{
		public static EnumSerializableContainer<TEnum1>[] ConvertTo<TEnum, TEnum1>(this EnumSerializableContainer<TEnum>[] values)
			where TEnum : unmanaged, Enum
			where TEnum1 : unmanaged, Enum
		{
			var result = new EnumSerializableContainer<TEnum1>[values.Length];
			for (var i = 0; i < values.Length; i++)
			{
				result[i] = values[i].ConvertTo<TEnum1>();
			}
			return result;
		}
	}
}
