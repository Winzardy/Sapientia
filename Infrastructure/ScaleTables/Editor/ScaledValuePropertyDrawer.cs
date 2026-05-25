#if UNITY_EDITOR
using Content;
using Content.ScriptableObjects;
using Content.ScriptableObjects.Editor;
using Content.ScriptableObjects.ScaleTables;
using Fusumity.Editor;
using Sapientia.Collections;
using Sapientia.Extensions;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Sapientia.ScaleTables.Editor
{
	public class ScaledValuePropertyDrawer : OdinValueDrawer<ScaledValue>
	{
		private GUIPopupSelector<string> _selector;
		private ScaleTableScriptableObject _scrobject;

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (label != null)
			{
				var style = new GUIStyle(GUI.skin.label);
				style.fontSize = 11;
				style.fontStyle = FontStyle.Bold;

				Rect rect = EditorGUILayout.GetControlRect();
				EditorGUI.LabelField(rect, label, style);
				SirenixEditorGUI.HorizontalLineSeparator(1);
			}

			GUILayout.BeginVertical(GUI.skin.box);

			foreach (var child in Property.Children)
			{
				if (child.Name == nameof(ScaledValue.rowIndex))
				{
					DrawRowIndex();
				}
				else
				{
					child.Draw();
				}
			}

			GUILayout.EndVertical();
		}

		private void DrawRowIndex()
		{
			var value = ValueEntry.SmartValue;

			if (value.scaleTableId.IsNullOrEmpty())
			{
				_scrobject = null;
				_selector = null;
				return;
			}

			if (_scrobject == null || _scrobject.Id != value.scaleTableId)
			{
				_scrobject = ContentDatabaseEditorUtility.GetScrobjectFromDb<ScaleTableScriptableObject, ScaleTablesDatabaseScriptableObject>(value.scaleTableId);

				if (_scrobject == null)
				{
					Debug.LogWarning($"Add selected table to database: [ {value.scaleTableId} ]");
					return;
				}

				var rowKeys = _scrobject.ScriptableContentEntry.Value.valueRows.Select(x => x.key).ToArray();
				var selected = rowKeys.WithinBounds(value.rowIndex) ? rowKeys[value.rowIndex] : rowKeys.First();

				_selector = new GUIPopupSelector<string>(rowKeys, selected, OnSelected);
			}

			var rect = EditorGUILayout.GetControlRect();
			_selector.DrawDropdown(rect);

			SuffixLabel(rect, $"index: {value.rowIndex}", 18f);
		}

		public static void SuffixLabel(Rect rect, string text, float padding = 8f)
		{
			GUI.Label(rect.HorizontalPadding(0.0f, padding), text, SirenixGUIStyles.RightAlignedGreyMiniLabel);
		}

		private void OnSelected(string rowKey, int index)
		{
			var value = ValueEntry.SmartValue;
			value.rowIndex = index;
			ValueEntry.SmartValue = value;
		}
	}
}
#endif

