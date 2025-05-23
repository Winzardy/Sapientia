using System;
using System.Diagnostics;

namespace Content
{
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class DocumentationAttribute :
#if CLIENT
		UnityEngine.HelpURLAttribute
#else
		Attribute
#endif

	{
		public DocumentationAttribute(string url)
#if CLIENT
			: base(url)
#endif
		{
		}
	}
}
