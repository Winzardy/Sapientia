#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia
{
	public enum LogicalOperator
	{
#if CLIENT
		[LabelText("||")]
		[Tooltip("Or (A или B)")]
#endif
		Or,
#if CLIENT
		[LabelText("&&")]
		[Tooltip("And (A и B)")]
#endif
		And
	}
}
