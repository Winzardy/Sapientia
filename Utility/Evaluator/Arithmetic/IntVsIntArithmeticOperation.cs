using System;
using System.Collections.Generic;
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
			return a.Evaluate(context)
				.Operate(@operator, b.Evaluate(context));
		}

		public override IEnumerator<IEvaluator> GetEnumerator()
		{
			yield return this;
			yield return a;
			if (!b.IsConstant)
				yield return b.evaluator;
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
