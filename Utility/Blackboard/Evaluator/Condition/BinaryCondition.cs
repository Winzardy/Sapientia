using System;
#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Boolean Operation",
		"/",
		SdfIconType.CodeSlash,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class BinaryCondition : InvertableBlackboardCondition
	{
#if CLIENT
		[HorizontalGroup(GROUP)]
		[HorizontalGroup(GROUP+"/group"), HideLabel]
#endif
		[SerializeReference]
		public BlackboardCondition a;

#if CLIENT
		[HorizontalGroup(GROUP+"/group", OPERATOR_WIDTH), HideLabel]
#endif
		public LogicalOperator @operator;

#if CLIENT
		[HorizontalGroup(GROUP+"/group"), HideLabel]
#endif
		[SerializeReference]
		public BlackboardCondition b;

		protected override bool OnEvaluate(Blackboard blackboard)
		{
			return @operator switch
			{
				LogicalOperator.Or => a.IsFulfilled(blackboard) || b.IsFulfilled(blackboard),
				LogicalOperator.And => a.IsFulfilled(blackboard) && b.IsFulfilled(blackboard),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
