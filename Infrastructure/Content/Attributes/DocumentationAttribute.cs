using System;
using System.Diagnostics;
using UnityEngine;

namespace Content
{
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class DocumentationAttribute : HelpURLAttribute
	{
		public DocumentationAttribute(string url) : base(url)
		{
		}
	}
}
