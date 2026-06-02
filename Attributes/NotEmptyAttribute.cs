using System;
using System.Diagnostics;

namespace Sapientia
{
	[Conditional("UNITY_EDITOR")]
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
	public class NotEmptyAttribute : Attribute
	{
	}
}
