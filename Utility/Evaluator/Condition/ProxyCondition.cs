using System;
using Sapientia.Evaluators;
using UnityEngine;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract class ProxyCondition<TContext1, TContext2> : Condition<TContext1>, IProxyEvaluator
	{
		[SerializeReference]
		public Condition<TContext2> value;

		protected override bool OnEvaluate(TContext1 context) => value?.IsFulfilled(Convert(context)) ?? true;

		protected abstract TContext2 Convert(TContext1 context);

		public IEvaluator Proxy => value;
	}
}
