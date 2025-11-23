using System;
using Sapientia.Extensions;

#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class IntVsIntArithmeticOperation<TContext> : Evaluator<TContext, int>
	{
#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		[SerializeReference]
		public Evaluator<TContext, int> a;

#if CLIENT
		[HorizontalGroup(OPERATOR_WIDTH), HideLabel]
#endif
		public ArithmeticOperator @operator;

#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		public EvaluatedValue<TContext, int> b;

		protected override int OnEvaluate(TContext context)
		{
			return @operator switch
			{
				ArithmeticOperator.Add => a.Evaluate(context) + b.Evaluate(context),
				ArithmeticOperator.Subtract => a.Evaluate(context) - b.Evaluate(context),
				ArithmeticOperator.Divide => a.Evaluate(context) / b.Evaluate(context),
				ArithmeticOperator.Multiply => a.Evaluate(context) % b.Evaluate(context),
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
				ArithmeticOperator.Modulus => IEvaluator.ARITHMETIC_OPERATOR_MODULUS,
				_ => throw new ArgumentOutOfRangeException()
			};

			return $"{a1}{o}{b1}";
		}
	}
}
