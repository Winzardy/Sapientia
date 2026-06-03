using System;
using System.Diagnostics;
using UnityEngine;

namespace Content
{
	[Conditional("UNITY_EDITOR")]
	public class ContentReferenceAttribute :
#if CLIENT
		PropertyAttribute
#else
		Attribute
#endif
	{
		public Type Type { get; private set; }
		public string TypeName { get; internal set;}
		public bool InlineEditor { get; private set;}

		public bool Dropdown { get; private set;}

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
