using System;
using System.Diagnostics;

namespace Sapientia
{
	[Conditional("UNITY_EDITOR")]
	public abstract class ParentAttribute : Attribute
	{
		public abstract Attribute Convert();
	}
}
