using System;
#if CLIENT
using UnityEngine;
using Sirenix.OdinInspector;
#endif

public enum ConditionGroupMode
{
	Any, // OR
	All // AND
}

namespace Sapientia.Conditions
{
	[Serializable]
#if CLIENT
	[TypeRegistryItem(
		"\u2009Collection",
		"/",
		SdfIconType.Stack,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A,
		priority: 100
	)]
#endif
	public sealed class CollectionCondition : InvertableBlackboardCondition
	{
		public ConditionGroupMode mode;

		[SerializeReference]
		public IBlackboardCondition[] collection = new IBlackboardCondition[0];

		protected override bool OnEvaluate(Blackboard blackboard)
		{
			switch (mode)
			{
				case ConditionGroupMode.Any:
					for (int i = 0; i < collection.Length; i++)
					{
						if (collection[i].Evaluate(blackboard))
							return true;
					}

					return false;
				case ConditionGroupMode.All:
					for (int i = 0; i < collection.Length; i++)
					{
						if (!collection[i].Evaluate(blackboard))
							return false;
					}

					return true;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
