using System;
using UnityEngine;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class BridgeEvaluator<TContext1, TContext2, TValue> : Evaluator<TContext1, TValue>, IBridgeEvaluator
	{
		[SerializeReference]
		public Evaluator<TContext2, TValue> value;

		protected override TValue OnEvaluate(TContext1 context) => value.Evaluate(Convert(context));

		protected abstract TContext2 Convert(TContext1 context);

		public IEvaluator Proxy => value;
		public Type ProxyType => typeof(Evaluator<TContext2, TValue>);

		public override string ToString() => value.ToString();
	}
}
