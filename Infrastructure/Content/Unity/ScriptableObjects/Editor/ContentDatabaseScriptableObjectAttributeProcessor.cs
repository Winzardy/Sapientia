﻿#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;

namespace Content.ScriptableObjects.Editor
{
	public class ContentDatabaseScriptableObjectAttributeProcessor : OdinAttributeProcessor<ContentDatabaseScriptableObject>
	{
		public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
			MemberInfo member, List<Attribute> attributes)
		{
			base.ProcessChildMemberAttributes(parentProperty, member, attributes);

			switch (member.Name)
			{
				case nameof(ContentDatabaseScriptableObject.scriptableObjects):
					attributes.Add(new SearchableAttribute());
					attributes.Add(new PropertyOrderAttribute(99));
					attributes.Add(new PropertySpaceAttribute(10));
					attributes.Add(new ListDrawerSettingsAttribute
					{
						IsReadOnly = true,
						OnTitleBarGUI = $"@{nameof(ContentDatabaseScriptableObjectAttributeProcessor)}." +
							$"{nameof(DrawSyncContent)}($property)"
					});
					break;
			}
		}

		private static void DrawSyncContent(InspectorProperty property)
		{
			if (!SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
				return;
			if (property.Parent.ValueEntry.WeakSmartValue is not ContentDatabaseScriptableObject database)
				return;
			EditorApplication.delayCall += OnDelayCall;

			void OnDelayCall()
			{
				EditorApplication.delayCall -= OnDelayCall;

				var origin = ContentDebug.Logging.database;
				try
				{
					ContentDebug.Logging.database = true;
					if (database is MiscDatabaseScriptableObject)
					{
						ContentDatabaseEditorUtility.SyncContent();
						return;
					}

					database.SyncContent();
				}
				finally
				{
					ContentDebug.Logging.database = origin;
				}
			}
		}
	}
}
#endif
