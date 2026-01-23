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
		public bool Foldout { get; }

		public bool Dropdown { get; }

		public ContentReferenceAttribute(Type type, bool foldout = true, bool dropdown = false)
		{
			Type = type;
			Foldout = foldout;
			Dropdown = dropdown;
		}

		public ContentReferenceAttribute(string typeName, bool foldout = true, bool dropdown = false)
		{
			TypeName = typeName;
			Foldout = foldout;
			Dropdown = dropdown;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public class ContentReferenceHideFoldoutAttribute : Attribute
	{
	}

	[Conditional("UNITY_EDITOR")]
	public class ContentReferenceDropdownAttribute : Attribute
	{
	}
}
