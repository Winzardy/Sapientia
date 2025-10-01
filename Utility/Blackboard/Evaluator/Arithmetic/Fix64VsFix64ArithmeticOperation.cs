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
	public class Fix64VsFix64ArithmeticOperation : BlackboardEvaluator<float>
	{
		[SerializeReference]
		public BlackboardEvaluator<Fix64> a;

		public ArithmeticOperator @operator;

		[SerializeReference]
		public BlackboardEvaluator<Fix64> b;

		protected override float OnGet(Blackboard blackboard)
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
