using System;
using System.Diagnostics;

namespace Content
{
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
	public class ContentPreviewAttribute : Attribute
	{
		public string IconSource { get; set; }

		public ContentPreviewAttribute(string iconSource = "PreviewIcon")
		{
			IconSource = iconSource;
		}
	}
}
