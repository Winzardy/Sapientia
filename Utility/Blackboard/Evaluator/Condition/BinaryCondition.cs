using System;
using UnityEngine;

namespace Sapientia
{
	[Serializable]
	public class BinaryCondition : Condition
	{
		[SerializeReference]
		public Condition a;

		public LogicalOperator @operator;

		[SerializeReference]
		public Condition b;

		protected override bool IsMet(Blackboard context)
		{
			return @operator switch
			{
				LogicalOperator.Or => a.Evaluate(context) || b.Evaluate(context),
				LogicalOperator.And => a.Evaluate(context) && b.Evaluate(context),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
