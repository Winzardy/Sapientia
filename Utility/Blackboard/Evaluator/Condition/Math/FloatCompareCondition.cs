using System;
using UnityEngine;

namespace Sapientia
{
	[Serializable]
	public class FloatCompareCondition : Condition
	{
		[SerializeReference]
		public IBlackboardEvaluator<float> a;

		public ComparisonOperator logicOperator;

		[SerializeReference]
		public IBlackboardEvaluator<float> b;

		protected override bool IsMet(Blackboard context)
		{
			return logicOperator switch
			{
				ComparisonOperator.GreaterOrEqual => a.Evaluate(context) >= b.Evaluate(context),
				ComparisonOperator.LessOrEqual => a.Evaluate(context) <= b.Evaluate(context),
				ComparisonOperator.Greater => a.Evaluate(context) > b.Evaluate(context),
				ComparisonOperator.Less => a.Evaluate(context) < b.Evaluate(context),
				ComparisonOperator.Equal => a.Evaluate(context) == b.Evaluate(context),
				ComparisonOperator.NotEqual => a.Evaluate(context) != b.Evaluate(context),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}