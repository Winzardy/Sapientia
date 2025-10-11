using System;
using Sapientia.Extensions;

#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Evaluator.Blackboard
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
	public class IntVsIntArithmeticOperation : BlackboardEvaluator<int>
	{
		[SerializeReference]
#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		public BlackboardEvaluator<int> a;

#if CLIENT
		[HorizontalGroup(OPERATOR_WIDTH), HideLabel]
#endif
		public ArithmeticOperator @operator;

		[SerializeReference]
#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		public BlackboardEvaluator<int> b;

		protected override int OnGet(Sapientia.Blackboard blackboard)
		{
			return @operator switch
			{
				ArithmeticOperator.Add => a.Get(blackboard) + b.Get(blackboard),
				ArithmeticOperator.Subtract => a.Get(blackboard) - b.Get(blackboard),
				ArithmeticOperator.Divide => a.Get(blackboard) / b.Get(blackboard),
				ArithmeticOperator.Multiply => a.Get(blackboard) * b.Get(blackboard),
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public override string ToString()
		{
			var a1 = a.ToString();
			var b1 = b.ToString();

			var o = @operator switch
			{
				ArithmeticOperator.Add => "+",
				ArithmeticOperator.Subtract => "-",
				ArithmeticOperator.Divide => "/",
				ArithmeticOperator.Multiply => "*",
				_ => throw new ArgumentOutOfRangeException()
			};

			return $"{a1}{o}{b1}";
		}
	}
}
