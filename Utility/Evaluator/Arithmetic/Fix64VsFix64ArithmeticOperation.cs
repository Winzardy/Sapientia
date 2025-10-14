using System;
using Sapientia.Deterministic;

#if CLIENT
using UnityEngine;
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Evaluators
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Arithmetic Operation",
		"Math",
		SdfIconType.PlusSlashMinus,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public abstract class Fix64VsFix64ArithmeticOperation<TContext> : Evaluator<TContext, Fix64>
	{
#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext,Fix64> a;

#if CLIENT
		[HorizontalGroup(OPERATOR_WIDTH), HideLabel]
#endif
		public ArithmeticOperator @operator;

#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		public EvaluatedValue<TContext,Fix64> b;

		protected override Fix64 OnGet(TContext context)
		{
			return @operator switch
			{
				ArithmeticOperator.Add => a.Get(context) + b.Get(context),
				ArithmeticOperator.Subtract => a.Get(context) - b.Get(context),
				ArithmeticOperator.Divide => a.Get(context) / b.Get(context),
				ArithmeticOperator.Multiply => a.Get(context) * b.Get(context),
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public override string ToString()
		{
			var a1 = a.ToString();
			var b1 = b.ToString();

			var o = @operator switch
			{
				ArithmeticOperator.Add => IEvaluator.ARITHMETIC_OPERATOR_ADD,
				ArithmeticOperator.Subtract => IEvaluator.ARITHMETIC_OPERATOR_SUBTRACT,
				ArithmeticOperator.Divide => IEvaluator.ARITHMETIC_OPERATOR_DIVIDE,
				ArithmeticOperator.Multiply => IEvaluator.ARITHMETIC_OPERATOR_MULTIPLY,
				_ => throw new ArgumentOutOfRangeException()
			};

			return $"{a1}{o}{b1}";
		}
	}
}
