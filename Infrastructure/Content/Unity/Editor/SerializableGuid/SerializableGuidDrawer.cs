#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Content.Editor
{
	public class SerializableGuidDrawer : OdinValueDrawer<SerializableGuid>
	{
		private bool _rawMode;
		protected override void DrawPropertyLayout(GUIContent label)
		{
			var output = ContentEditorGUILayout.DrawGuidField(ValueEntry.SmartValue, label, ref _rawMode);

			if (!GUI.enabled)
				return;

			ValueEntry.SmartValue = output;
		}
	}
}
#endif
