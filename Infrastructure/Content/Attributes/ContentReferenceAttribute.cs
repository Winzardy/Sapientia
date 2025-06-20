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
		public bool Foldout { get; }

		public ContentReferenceAttribute(Type type, bool foldout = true)
		{
			Type = type;
			Foldout = foldout;
		}
	}

	[Conditional("UNITY_EDITOR")]
	public class HideFoldoutAttribute : Attribute
	{
	}
}
