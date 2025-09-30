using System;
using Sapientia.Deterministic;

#if CLIENT
using UnityEngine;
using Sirenix.OdinInspector;
#endif

namespace Sapientia.BlackboardEvaluator
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Float Operation",
		"Math",
		SdfIconType.ArrowLeftRight,
		darkIconColorR: R, darkIconColorG: G,
		darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G,
		lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class Fix64VsFix64ArithmeticOperation : BlackboardEvaluator<float>
	{
		[SerializeReference]
		public IBlackboardEvaluator<Fix64> a;

		public ArithmeticOperator @operator;

		[SerializeReference]
		public IBlackboardEvaluator<Fix64> b;

		protected override float OnGet(Blackboard blackboard)
		{
			return @operator switch
			{
				ArithmeticOperator.Add => a.Evaluate(blackboard) + b.Evaluate(blackboard),
				ArithmeticOperator.Subtract => a.Evaluate(blackboard) - b.Evaluate(blackboard),
				ArithmeticOperator.Divide => a.Evaluate(blackboard) / b.Evaluate(blackboard),
				ArithmeticOperator.Multiply => a.Evaluate(blackboard) * b.Evaluate(blackboard),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
