#if UNITY_EDITOR
using Content.ScriptableObjects;
using JetBrains.Annotations;
using Sapientia.Editor;
using Sapientia.Reflection;
using Sirenix.OdinInspector.Editor;

namespace Content.Editor
{
	public static partial class ContentEntryEditorUtility
	{
		[CanBeNull]
		public static MemberReflectionReference<IUniqueContentEntry> ToContentReference(this InspectorProperty property)
		{
			//TODO: нужно чтобы Nested обязательно лежали в IContentEntry... (подумать)
			if (!property.Path.Contains(IContentEntrySource.ENTRY_FIELD_NAME))
				return null;

			//skipSteps = 1 потому что пропускаем _entry
			var reference = property.ToReference<IUniqueContentEntry>(1);
			return reference.FixSerializeReference();
		}

		public static void RegenerateGuid(this InspectorProperty property, IUniqueContentEntry entry, ContentScriptableObject asset)
		{
			RegenerateGuid(entry, property.Path, asset);
			RecursiveRegenerateGuidForChildren(property, asset);

			property.MarkSerializationRootDirty();
		}

		public static void RestoreGuid(this InspectorProperty property, IUniqueContentEntry entry, in SerializableGuid guid) =>
			RestoreGuid(entry, in guid, property.Path, property.Tree.UnitySerializedObject.targetObject);

		private static void RecursiveRegenerateGuidForChildren(this InspectorProperty property, ContentScriptableObject asset)
		{
			property.Children.Update();
			foreach (var child in property.Children.Recurse())
			{
				if (child.ValueEntry?.WeakSmartValue is IUniqueContentEntry entry)
					RegenerateGuid(entry, child.Path, asset);
			}
		}
	}
}
#endif
