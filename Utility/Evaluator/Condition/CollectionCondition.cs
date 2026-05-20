using System;
using System.Collections.Generic;
#if CLIENT
using UnityEngine;
using Sirenix.OdinInspector;
#endif

namespace Sapientia.Conditions
{
	public enum ConditionGroupMode
	{
		Any, // OR
		All // AND
	}

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
						if (collection[i].IsFulfilled(context))
							return true;
					}

					return false;
				case ConditionGroupMode.All:
					for (int i = 0; i < collection.Length; i++)
					{
						if (!collection[i].IsFulfilled(context))
							return false;
					}

					return true;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public override IEnumerator<IEvaluator> GetEnumerator()
		{
			yield return this;
			for (int i = 0; i < collection.Length; i++)
				yield return collection[i];
		}
	}
}
