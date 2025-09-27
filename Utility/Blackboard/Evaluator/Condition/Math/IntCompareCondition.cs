using System;
#if CLIENT
using UnityEngine;
#endif
namespace Sapientia
{
	[Serializable]
	public class IntCompareCondition : Condition
	{
		[SerializeReference]
		public IBlackboardEvaluator<int> a;

		public ComparisonOperator logicOperator;

		[SerializeReference]
		public IBlackboardEvaluator<int> b;

		protected override bool IsMet(Blackboard blackboard)
		{
			return logicOperator switch
			{
				ComparisonOperator.GreaterOrEqual => a.Evaluate(blackboard) >= b.Evaluate(blackboard),
				ComparisonOperator.LessOrEqual => a.Evaluate(blackboard) <= b.Evaluate(blackboard),
				ComparisonOperator.Greater => a.Evaluate(blackboard) > b.Evaluate(blackboard),
				ComparisonOperator.Less => a.Evaluate(blackboard) < b.Evaluate(blackboard),
				ComparisonOperator.Equal => a.Evaluate(blackboard) == b.Evaluate(blackboard),
				ComparisonOperator.NotEqual => a.Evaluate(blackboard) != b.Evaluate(blackboard),
				_ => throw new NotImplementedException(),
			};
		}
	}

	// [Serializable]
	// public class BlackboardValue<T> : IBlackboardEvaluator<T>
	// {
	// 	private const string CATALOG_ID = "BlackboardKeys";
	//
	// 	//	[ContextLabel(CATALOG_ID)]
	// 	public Toggle<string> key;
	//
	// 	public T Evaluate(Blackboard context)
	// 	{
	// 		return key
	// 			? context.Get<T>(key)
	// 			: context.Get<T>();
	// 	}
	// }
}
