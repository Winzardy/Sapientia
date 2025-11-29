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
	public abstract class CollectionCondition<T> : InvertableCondition<T>
	{
		public ConditionGroupMode mode;

		[SerializeReference]
		public Condition<T>[] collection = new Condition<T>[0];

		protected override bool OnEvaluate(T context)
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
