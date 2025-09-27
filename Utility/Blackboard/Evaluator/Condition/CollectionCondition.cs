using System;
#if CLIENT
using UnityEngine;
#endif

namespace Sapientia
{
	public enum ConditionGroupMode
	{
		Any, // OR
		All // AND
	}

	[Serializable]
	public class CollectionCondition : Condition
	{
		public ConditionGroupMode mode;

		[SerializeReference]
		public Condition[] collection = new Condition[0];

		protected override bool IsMet(Blackboard context)
		{
			switch (mode)
			{
				case ConditionGroupMode.Any:
					for (int i = 0; i < collection.Length; i++)
					{
						if (collection[i].Evaluate(context))
							return true;
					}

					return false;
				case ConditionGroupMode.All:
					for (int i = 0; i < collection.Length; i++)
					{
						if (!collection[i].Evaluate(context))
							return false;
					}

					return true;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
