using System;
using UnityEngine;

namespace Sapientia
{
	[Serializable]
	public class IntVsIntArithmeticOperation : IBlackboardEvaluator<int>
	{
		[SerializeReference]
		public IBlackboardEvaluator<int> a;

		public ArithmeticOperator @operator;

		[SerializeReference]
		public IBlackboardEvaluator<int> b;

		public int Evaluate(Blackboard blackboard)
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