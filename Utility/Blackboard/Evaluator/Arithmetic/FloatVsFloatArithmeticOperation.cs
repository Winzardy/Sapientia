using System;
using UnityEngine;

namespace Sapientia
{
	[Serializable]
	public class FloatVsFloatArithmeticOperation : IBlackboardEvaluator<float>
	{
		[SerializeReference]
		public IBlackboardEvaluator<float> a;

		public ArithmeticOperator @operator;

		[SerializeReference]
		public IBlackboardEvaluator<float> b;

		public float Evaluate(Blackboard blackboard)
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