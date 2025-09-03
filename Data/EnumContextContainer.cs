using System;
using System.Collections.Generic;
using Sapientia.Extensions;

namespace Submodules.Sapientia.Data
{
	/// <summary>
	/// Тип `TEnumContainer` - это generic, который в качестве одного из `generic` аргументов содержит `enum` тип.
	/// - Если такого аргумента нет, то контейнер будет пустым.
	/// `TContext` является наследником `TEnumContainer`, именно в нём происходит поиск экземпляра базового типа, из которого достаётся `enum` тип.
	/// Этот `enum` тип используется для отрисовки `enum` - селектора.
	/// </summary>
	/// <typeparam name="TContext"></typeparam>
	/// <typeparam name="TEnumContainer"> Должен быть `generic`, который содержит `TEnum`!</typeparam>
#if UNITY_EDITOR
	[Sirenix.OdinInspector.InlineProperty]
#endif
	[Serializable]
	public struct EnumContextContainer<TContext, TEnumContainer>
#if UNITY_EDITOR
		: UnityEngine.ISerializationCallbackReceiver
#endif
		where TContext : TEnumContainer
	{
		/// <summary>
		/// Поле предназначено для правильной сериализации на случай, если список статов будет изменён.
		/// </summary>
#if UNITY_EDITOR
		[UnityEngine.HideInInspector]
		public string enumValueName;

		[Sirenix.OdinInspector.ShowIf(nameof(ShowValueCondition))]
		[Sirenix.OdinInspector.ValueDropdown(nameof(GetEnumItems))]
		[Sirenix.OdinInspector.HideLabel]
#endif
		public int value;

		public static EnumContextContainer<TContext, TEnumContainer> Default
		{
			get
			{
#if UNITY_EDITOR
				var enumCollection = GetEnumCollection();
				foreach (var (name, value) in enumCollection.enumValues)
				{
					return new EnumContextContainer<TContext, TEnumContainer>()
					{
						enumValueName = name,
						value = value,
					};
				}
#endif
				return default;
			}
		}

		public static implicit operator int(EnumContextContainer<TContext, TEnumContainer> container)
		{
			return container.value;
		}

		public static implicit operator EnumContextContainer<TContext, TEnumContainer>(int value)
		{
			return new EnumContextContainer<TContext, TEnumContainer>{ value = value };
		}

		public EnumContextContainer<TC, TEC> ConvertTo<TC, TEC>()
			where TC : TEC
		{
			return new EnumContextContainer<TC, TEC>
			{
				value = value,
#if UNITY_EDITOR
				enumValueName = enumValueName,
#endif
			};
		}

#if UNITY_EDITOR
		public static IEnumerable<Sirenix.OdinInspector.ValueDropdownItem> GetEnumItems()
		{
			var enumCollection = GetEnumCollection();

			foreach (var (name, value) in enumCollection.enumValues)
			{
				yield return new Sirenix.OdinInspector.ValueDropdownItem(name, value);
			}
		}

		private bool ShowValueCondition()
		{
			var enumCollection = GetEnumCollection();
			return enumCollection.enumNames.Count > 1;
		}

		private bool ShowErrorMessage()
		{
			var enumCollection = GetEnumCollection();
			return enumCollection.enumNames.Count < 1;
		}

		public void OnBeforeSerialize()
		{
			var enumCollection = GetEnumCollection();
			enumValueName = enumCollection.GetName(value);
		}

		public void OnAfterDeserialize()
		{
			var enumCollection = GetEnumCollection();
			if (enumValueName.IsNullOrEmpty() || !enumCollection.enumValues.TryGetValue(enumValueName, out var enumValue))
			{
				enumValueName = enumCollection.GetName(value);
			}
			else if (!value.Equals(enumValue))
			{
				value = enumValue;
			}
		}

		private static EnumTypeExt.EnumCollection GetEnumCollection()
		{
			return EnumTypeExt.GetEnumCollectionFromContext(typeof(TContext), typeof(TEnumContainer));
		}
#endif
	}
}
