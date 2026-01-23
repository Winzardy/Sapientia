using System;
using System.Diagnostics;

namespace Content
{
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Field)]
	public class ContentReferenceAttribute :
#if CLIENT
		UnityEngine.PropertyAttribute
#else
		Attribute
#endif
	{
		public Type Type { get; }
		public string TypeName { get; }
		public bool InlineEditor { get; }

		public bool Dropdown { get; }

		public ContentReferenceAttribute(Type type, bool inlineEditor = true, bool dropdown = false)
		{
			Type = type;
			InlineEditor = inlineEditor;
			Dropdown = dropdown;
		}

		public ContentReferenceAttribute(string typeName, bool inlineEditor = true, bool dropdown = false)
		{
			TypeName = typeName;
			InlineEditor = inlineEditor;
			Dropdown = dropdown;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public class ContentReferenceDrawerSettingsAttribute : Attribute
	{
		public bool InlineEditor { get; }
		public bool Dropdown { get; }

		public ContentReferenceDrawerSettingsAttribute(bool inlineEditor = true, bool dropdown = false)
		{
			InlineEditor = inlineEditor;
			Dropdown = dropdown;
		}
	}
}
