using System;
using System.Diagnostics;

namespace Content
{
	[Conditional("CLIENT")]
	public class ClientOnlyAttribute : Attribute
	{
	}
}
