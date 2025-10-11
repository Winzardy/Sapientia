#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia
{
	public enum ArithmeticOperator
	{
#if CLIENT
		[LabelText("+")]
		[Tooltip("Сложение (A плюс B)")]
#endif
		Add,
#if CLIENT
		[LabelText("\u2212")]
		[Tooltip("Вычитание (A минус B)")]
#endif
		Subtract,
#if CLIENT
		[LabelText("\u00f7")]
		[Tooltip("Деление (A делить на B)")]
#endif
		Divide,
#if CLIENT
		[LabelText("\u00d7")]
		[Tooltip("Умножение (A умножить на B)")]
#endif
		Multiply
	}
}
