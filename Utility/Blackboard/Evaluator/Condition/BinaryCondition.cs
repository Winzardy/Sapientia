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
		"\u2009Binary",
		"Common",
		SdfIconType.Gear,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A
	)]
#endif
	public class BinaryCondition : Condition
	{
#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		[SerializeReference]
		public Condition a;

#if CLIENT
		[HorizontalGroup(40), HideLabel]
#endif
		public LogicalOperator @operator;

#if CLIENT
		[HorizontalGroup, HideLabel]
#endif
		[SerializeReference]
		public Condition b;

		protected override bool OnEvaluate(Blackboard context)
		{
			return @operator switch
			{
				LogicalOperator.Or => a.IsMet(context) || b.IsMet(context),
				LogicalOperator.And => a.IsMet(context) && b.IsMet(context),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
