﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Content.Editor;
using Generic.Extensions;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Content.ScriptableObjects.Editor
{
	public class ContentScriptableObjectAttributeProcessor : OdinAttributeProcessor<ContentScriptableObject>
	{
		private static readonly string LABEL = "Guid";

		private static readonly string TOOLTIP_PREFIX = $"{LABEL}:\n".ColorText(Color.gray).SizeText(12);

		private const string NAME_SEPARATOR = "_";
		private const string SMART_NAME_SEPARATOR = "/";
		private const string ERROR_MESSAGE = "Can only set a new ID in the root inspector!";

		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
			MemberInfo member, List<Attribute> attributes)
		{
			base.ProcessChildMemberAttributes(parentProperty, member, attributes);

			var rootClass = nameof(ContentScriptableObjectAttributeProcessor);
			switch (member.Name)
			{
				case ContentEntryScriptableObject.CUSTOM_ID_FIELD_NAME:

					attributes.Add(new LabelTextAttribute("Id"));
					attributes.Add(new PropertyOrderAttribute(-99));
					attributes.Add(new VerticalGroupAttribute("Identifier"));
					attributes.Add(new ShowInInspectorAttribute());
					attributes.Add(new TooltipAttribute($"@{rootClass}.{nameof(GetTooltip)}($property)"));

					if (parentProperty.SerializationRoot.ValueEntry.WeakSmartValue is not IUniqueContentEntryScriptableObject so)
						return;

					var scriptable = so.ScriptableContentEntry.ScriptableObject;

					attributes.Add(new EnableIfAttribute("@EditorGUIHelper.drawAssetReference"));

					if (scriptable && !scriptable.name.IsNullOrEmpty())
					{
						var split = scriptable.name.Split(NAME_SEPARATOR);

						for (var i = 1; i < split.Length; i++)
						{
							attributes.Add(new CustomContextMenuAttribute(
								$"Set/{GetSmartName(scriptable.name, i, true)}",
								$"@{rootClass}.{nameof(SetSmart)}($property, {i})"));
						}
					}

					//attributes.Add(new SuffixLabelAttribute($"@{rootClass}.{nameof(Suffix)}($property)"));

					attributes.Add(new CustomContextMenuAttribute(
						$"Copy Guid",
						$"@{rootClass}.{nameof(CopyGuid)}($property)"));
					attributes.Add(new DelayedPropertyAttribute());
					attributes.Add(new OnValueChangedAttribute(
						$"@{rootClass}.{nameof(OnIdChanged)}($property)"));
					break;

				case nameof(ContentScriptableObject.creationTimeStr):
					attributes.Add(new TooltipAttribute(ContentScriptableObject.CREATION_TIME_TOOLTIP));
					attributes.Add(new LabelTextAttribute(nameof(ContentScriptableObject.creationTime), true));
					attributes.Add(new PropertyOrderAttribute(-1));
					attributes.Add(new ShowInInspectorAttribute());
					attributes.Add(new ShowIfAttribute($"@{nameof(ContentScriptableObjectAttributeProcessor)}.{nameof(IsDebugMode)}()"));
					attributes.Add(new ReadOnlyAttribute());
					attributes.Add(new CustomContextMenuAttribute(
						"Force Update",
						$"@{rootClass}.{nameof(ForceUpdateTimeCreated)}($property)"));
					break;

				case ContentEntryScriptableObject.USE_CUSTOM_ID_FIELD_NAME:
				case ContentScriptableObject.TIME_CREATED_FILED_NAME:
					attributes.Add(new HideInInspector());
					break;

				case ContentEntryScriptableObject.GUID_FIELD_NAME:
					attributes.Add(new PropertySpaceAttribute(-1.5f));
					attributes.Add(new ShowIfAttribute($"@{nameof(ContentScriptableObjectAttributeProcessor)}.{nameof(IsDebugMode)}()"));
					attributes.Add(new ShowInInspectorAttribute());
					attributes.Add(new VerticalGroupAttribute("Identifier"));
					break;
				case IContentEntrySource.ENTRY_FIELD_NAME:
					attributes.Add(new HideLabelAttribute());
					attributes.Add(new PropertySpaceAttribute(4));
					break;
			}
		}

		public static bool IsDebugMode() => ContentEntryDebugModeMenu.IsEnable;

		public static void OnIdChanged(InspectorProperty property)
		{
			if (property.SerializationRoot.ValueEntry.WeakSmartValue is not IUniqueContentEntryScriptableObject
			    {
				    UseCustomId: true
			    } entryScriptableObject)
				return;

			ContentAutoConstantsGenerator.ForceInvokeWithDelay(entryScriptableObject.GetType());
		}

		public static string GetTooltip(InspectorProperty property)
		{
			if (property.SerializationRoot.ValueEntry.WeakSmartValue is IUniqueContentEntrySource entryScriptableObject)
				return TOOLTIP_PREFIX + entryScriptableObject.UniqueContentEntry.Guid;

			return string.Empty;
		}

		public static void CopyGuid(InspectorProperty property)
		{
			if (property.SerializationRoot.ValueEntry.WeakSmartValue is not IUniqueContentEntryScriptableObject so)
				return;

			Clipboard.Copy(so.UniqueContentEntry.Guid.ToString());
		}

		public static string Suffix(InspectorProperty property)
		{
			if (property.SerializationRoot.ValueEntry.WeakSmartValue is not IUniqueContentEntryScriptableObject so)
				return string.Empty;

			if (!so.UseCustomId)
				return string.Empty;

			return so.UniqueContentEntry.Guid.ToString();
		}

		public static void ForceUpdateTimeCreated(InspectorProperty property)
		{
			if (property.SerializationRoot.ValueEntry.WeakSmartValue is not ContentScriptableObject so)
				return;

			so.ForceUpdateTimeCreated();
		}

		public static void SetSmart(InspectorProperty property, int depth)
		{
			if (property.SerializationRoot.ValueEntry.WeakSmartValue is not ContentScriptableObject so)
				return;

			if (property.ValueEntry.WeakSmartValue is Toggle<string> id)
			{
				var name = GetSmartName(so.name, depth);

				if (id == name)
					return;

				property.ValueEntry.WeakSmartValue = new Toggle<string>(name);
				EditorUtility.SetDirty(so);
			}
		}

		private static string GetSmartName(string fullName, int depth, bool editor = false)
		{
			var split = fullName.Split(NAME_SEPARATOR);

			if (split.Length > depth)
			{
				var strings = split
				   .ToList()
				   .GetRange(split.Length - depth, depth)
				   .ToArray();

				return string.Join(editor ? "  \u0338 " : SMART_NAME_SEPARATOR, strings);
			}

			return split[^1];
		}
	}
}
#endif
