using System;
#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions.Common
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Boolean Operation",
		"Common",
		SdfIconType.CodeSlash,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class BinaryCondition : Condition
	{
#if CLIENT
		[HorizontalGroup(GROUP)]
		[HorizontalGroup(GROUP+"/group"), HideLabel]
#endif
		[SerializeReference]
		public Condition a;

#if CLIENT
		[HorizontalGroup(GROUP+"/group", 45), HideLabel]
#endif
		public LogicalOperator @operator;

#if CLIENT
		[HorizontalGroup(GROUP+"/group"), HideLabel]
#endif
		[SerializeReference]
		public Condition b;

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
