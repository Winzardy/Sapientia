#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace Sapientia.Data.Time
{
	[CustomPropertyDrawer(typeof(LongDateTimeAttribute))]
	public class LongDateTimeDrawer : PropertyDrawer
	{
		const float LabelWidth = 30f;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			var ticks = property.longValue;
			var dt = ticks > 0 ? new DateTime(ticks, DateTimeKind.Unspecified) : DateTime.Now;

			var year = dt.Year;
			var month = dt.Month;
			var day = dt.Day;
			var hour = dt.Hour;
			var minute = dt.Minute;
			var second = dt.Second;

			var fullW = position.width;
			var colW = fullW / 3f;
			var h = EditorGUIUtility.singleLineHeight;

			// Row 1: Year / Month / Day
			DrawLabeledIntField(position.x, position.y, colW, "Y", ref year);
			DrawLabeledIntField(position.x + colW, position.y, colW, "M", ref month);
			DrawLabeledIntField(position.x + colW * 2f, position.y, colW, "D", ref day);

			// Row 2: Hour / Minute / Second
			var row2Y = position.y + h + 2;

			DrawLabeledIntField(position.x, row2Y, colW, "H", ref hour);
			DrawLabeledIntField(position.x + colW, row2Y, colW, "m", ref minute);
			DrawLabeledIntField(position.x + colW * 2f, row2Y, colW, "s", ref second);

			try
			{
				dt = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
				property.longValue = dt.Ticks;
			}
			catch
			{
				Debug.LogError("Wrong DateTime! Was reset to a previous value.");
			}

			EditorGUI.EndProperty();
		}

		private void DrawLabeledIntField(float x, float y, float width, string label, ref int value)
		{
			var h = EditorGUIUtility.singleLineHeight;

			var labelRect = new Rect(x, y, LabelWidth, h);
			var fieldRect = new Rect(x + LabelWidth, y, width - LabelWidth, h);

			EditorGUI.LabelField(labelRect, label);
			value = EditorGUI.IntField(fieldRect, value);
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return EditorGUIUtility.singleLineHeight * 2 + 4;
		}
	}
}
#endif
