using System;
#if CLIENT
using Sirenix.OdinInspector;
using UnityEngine;
#endif

namespace Sapientia.Conditions
{
	public enum LogicalOperator
	{
#if CLIENT
		[Tooltip("A или B")]
#endif
		Or,
#if CLIENT
		[Tooltip("A и B")]
#endif
		And
	}

	[Serializable]
	public abstract class BinaryCondition<TContext> : InvertableCondition<TContext>
	{
#if CLIENT
		[HorizontalGroup(GROUP)]
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Condition<TContext> a;

#if CLIENT
		[HorizontalGroup(GROUP + "/group", OPERATOR_WIDTH + 8), HideLabel]
#endif
		public LogicalOperator @operator;

#if CLIENT
		[HorizontalGroup(GROUP + "/group"), HideLabel]
#endif
		[SerializeReference]
		public Condition<TContext> b;

		protected override bool OnEvaluate(TContext context)
		{
			return @operator switch
			{
				LogicalOperator.Or => a.IsFulfilled(context) || b.IsFulfilled(context),
				LogicalOperator.And => a.IsFulfilled(context) && b.IsFulfilled(context),
				_ => throw new ArgumentOutOfRangeException()
			};
		}
	}
}
