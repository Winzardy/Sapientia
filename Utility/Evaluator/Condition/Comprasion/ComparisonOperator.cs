#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Comparison
{
	public enum ComparisonOperator
	{
#if CLIENT
		[LabelText("\u2265")]
		[Tooltip("Больше или равно")]
#endif
		GreaterOrEqual,
#if CLIENT
		[LabelText("\u2264")]
		[Tooltip("Меньше или равно")]
#endif
		LessOrEqual,
#if CLIENT
		[LabelText(">")]
		[Tooltip("Больше")]
#endif
		Greater,
#if CLIENT
		[LabelText("<")]
		[Tooltip("Меньше")]
#endif
		Less,
#if CLIENT
		[LabelText("\uff1d")]
		[Tooltip("Равно")]
#endif
		Equal,
#if CLIENT
		[LabelText("\u2260")]
		[Tooltip("Не равно")]
#endif
		NotEqual,
	}
}
