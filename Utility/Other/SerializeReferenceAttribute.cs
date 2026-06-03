#if !CLIENT
using System;
using System.Diagnostics;

namespace UnityEngine
{
	/// Надоело везде писать UnityEngine.SerializeReference и оборачивать в дефайны
	[Conditional("CLIENT")]
	public class SerializeReferenceAttribute : Attribute
	{
	}

	[Conditional("CLIENT")]
	public class TooltipAttribute: Attribute
	{
		public TooltipAttribute(string tooltip)
		{
		}
	}

	[Conditional("CLIENT")]
	public class SpaceAttribute: Attribute
	{
		public SpaceAttribute()
		{
		}
	}
}

/// Надоело везде писать UnityEngine.SerializeReference и оборачивать в дефайны
[Conditional("CLIENT")]
public class SerializeReferenceAttribute : Attribute
{
}
#endif
