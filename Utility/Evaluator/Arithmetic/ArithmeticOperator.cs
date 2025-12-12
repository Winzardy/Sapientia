#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia
{
	public enum ArithmeticOperator
	{
#if CLIENT
		[LabelText(IEvaluator.ARITHMETIC_OPERATOR_ADD)]
		[Tooltip("Сложение (A плюс B)")]
#endif
		Add,
#if CLIENT
		[LabelText(IEvaluator.ARITHMETIC_OPERATOR_SUBTRACT)]
		[Tooltip("Вычитание (A минус B)")]
#endif
		Subtract,
#if CLIENT
		[LabelText(IEvaluator.ARITHMETIC_OPERATOR_DIVIDE)]
		[Tooltip("Деление (A делить на B)")]
#endif
		Divide,
#if CLIENT
		[LabelText(IEvaluator.ARITHMETIC_OPERATOR_MULTIPLY)]
		[Tooltip("Умножение (A умножить на B)")]
#endif
		Multiply,
#if CLIENT
		[LabelText(IEvaluator.ARITHMETIC_OPERATOR_MODULUS)]
		[Tooltip("Остаток от деления (A % B)")]
#endif
		Modulus,
	}
}
