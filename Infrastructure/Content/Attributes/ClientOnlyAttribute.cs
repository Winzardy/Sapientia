using System;
using System.Diagnostics;

namespace Content
{
	[Conditional("UNITY_EDITOR")]
	public class ClientOnlyAttribute : Attribute
	{
	}
}
