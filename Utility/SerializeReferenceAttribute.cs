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
}
#endif
