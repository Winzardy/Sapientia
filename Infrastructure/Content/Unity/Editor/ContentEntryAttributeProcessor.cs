﻿#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Sapientia;
using Sapientia.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Content.Editor
{
	public class DisableContentEntryDrawerAttribute : Attribute
	{
	}

	public class ContentEntryAttributeProcessor : OdinAttributeProcessor<IContentEntry>
	{
		private const string LIST_DEFAULT_LABEL = "List";
		private const string ARRAY_DEFAULT_LABEL = "Array";

		private static readonly string LABEL_GUID = "Guid";

		public static readonly string TOOLTIP_PREFIX_GUID = $"{LABEL_GUID}:\n".ColorText(Color.gray).SizeText(12);

		private static readonly Dictionary<InspectorProperty, GUIContent> propertyToGUIContent = new();

		public override bool CanProcessSelfAttributes(InspectorProperty property)
		{
			var type = property.ValueEntry.TypeOfValue;
			return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ContentEntry<>);
		}

		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
			MemberInfo member, List<Attribute> attributes)
		{
			base.ProcessChildMemberAttributes(parentProperty, member, attributes);

			if (parentProperty.ValueEntry.WeakSmartValue is not IContentEntry contentEntry)
				return;

			switch (member.Name)
			{
				case IContentEntry.VALUE_FIELD_NAME:

					//TODO: Добавить свой AssetSelector...
					//if (typeof(Component).IsAssignableFrom(contentEntry.ValueType))
						//attributes.Add(new Fusumity.Attributes.Specific.FastAssetSelectorAttribute());

					if (contentEntry.ValueType.IsSerializeReference())
					{
						attributes.Add(new HideInInspector());
					}
					else
					{
						if (propertyToGUIContent.TryGetValue(parentProperty, out var content))
						{
							if (!content.text.IsNullOrEmpty())
								attributes.Add(new LabelTextAttribute(content.text));
							else
								attributes.Add(new HideLabelAttribute());

							if (!content.tooltip.IsNullOrEmpty())
								attributes.Add(new TooltipAttribute(content.tooltip));

							if (!parentProperty.Attributes.HasAttribute<DisableContentEntryDrawerAttribute>())
								attributes.Add(new TooltipAttribute(
									$"@{nameof(ContentEntryAttributeProcessor)}.{nameof(GetTooltip)}($property, \"{content.tooltip}\")"));
						}
						else
						{
							attributes.Add(new HideLabelAttribute());
						}

						attributes.Add(new CustomContextMenuAttribute(
							"Copy Guid",
							$"@{nameof(ContentEntryAttributeProcessor)}.{nameof(CopyGuid)}($property)"));
					}

					break;

				case IContentEntry.UNITY_VALUE_FIELD_NAME:
				case IUniqueContentEntry.GUID_FIELD_NAME:
					attributes.Add(new HideInInspector());

					break;

				case IContentEntry.CUSTOM_VALUE_FIELD_NAME:
					if (!contentEntry.ValueType.IsSerializeReference())
					{
						attributes.Add(new HideInInspector());
					}
					else
					{
						attributes.Add(new ShowInInspectorAttribute());

						if (propertyToGUIContent.TryGetValue(parentProperty, out var label) && label.text != null)
						{
							attributes.Add(new LabelTextAttribute(label.text));
							if (!parentProperty.Attributes.HasAttribute<DisableContentEntryDrawerAttribute>())
								attributes.Add(new TooltipAttribute(
									$"@{nameof(ContentEntryAttributeProcessor)}.{nameof(GetTooltip)}($property, \"{label.tooltip}\")"));
						}
						else
							attributes.Add(new HideLabelAttribute());

						attributes.Add(new CustomContextMenuAttribute(
							"Copy Guid",
							$"@{nameof(ContentEntryAttributeProcessor)}.{nameof(CopyGuid)}($property)"));
					}

					break;
			}
		}

		public static void CopyGuid(InspectorProperty property)
		{
			if (property.Parent.ValueEntry.WeakSmartValue is not IUniqueContentEntry contentEntry)
				return;

			Clipboard.Copy(contentEntry.Guid.ToString());
		}

		public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
		{
			base.ProcessSelfAttributes(property, attributes);

			var guiContent = new GUIContent(property.Label);
			propertyToGUIContent[property] = guiContent;
			var valueType = property.ValueEntry.TypeOfValue.GetGenericArguments()[0];

			var isCollection = typeof(IList).IsAssignableFrom(valueType);
			var collectionLabel = valueType.IsArray ? ARRAY_DEFAULT_LABEL : LIST_DEFAULT_LABEL;
			if (attributes.GetAttribute<HideLabelAttribute>() != null ||
			    typeof(IList).IsAssignableFrom(property.ValueEntry.ParentType))
				guiContent.text = isCollection ? collectionLabel : string.Empty;
			else if (attributes.GetAttribute<LabelTextAttribute>() != null)
				guiContent.text = attributes.GetAttribute<LabelTextAttribute>().Text;
			else if (property.Label.text.IsNullOrEmpty() && isCollection)
				guiContent.text = collectionLabel;

			if (attributes.GetAttribute<TooltipAttribute>() != null)
				guiContent.tooltip = attributes.GetAttribute<TooltipAttribute>().tooltip;

			attributes.Add(new HideLabelAttribute());

			if (!property.Attributes.HasAttribute<DisableContentEntryDrawerAttribute>())
				attributes.Add(new ContentEntryGroupStyleAttribute(property));

			attributes.RemoveAll(attr => attr is LabelTextAttribute);

			attributes.Add(new HideReferenceObjectPickerAttribute());
		}

		public static string GetTooltip(InspectorProperty property, string tooltip)
		{
			if (property.Parent?.ValueEntry?.WeakSmartValue is IUniqueContentEntry contentEntry)
			{
				if (!tooltip.IsNullOrEmpty())
					tooltip += ContentReferenceDrawer.TOOLTIP_SPACE;

				tooltip += $"{TOOLTIP_PREFIX_GUID}{contentEntry.Guid}";
			}

			return tooltip;
		}
	}

	internal class ContentEntryGroupStyleAttribute : Attribute
	{
		public InspectorProperty InspectorProperty { get; }

		public ContentEntryGroupStyleAttribute(InspectorProperty property)
		{
			InspectorProperty = property;
		}
	}
}
#endif
