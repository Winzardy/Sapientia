using System;
using Sapientia.Evaluators.Tracking;
using UnityEngine;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class BridgeEvaluator<TContext1, TContext2, TValue> : Evaluator<TContext1, TValue>,
		IBridgeEvaluator<TContext1, TContext2, TValue>,
		ITrackableEvaluator
		where TContext1 : class
		where TContext2 : class
	{
		[SerializeReference]
		public Evaluator<TContext2, TValue> value;

		public IEvaluator Proxy { get => value; }
		public Type ProxyType { get => typeof(Evaluator<TContext2, TValue>); }

		public Type TrackerType { get => typeof(BridgeEvaluatorTracker<TContext1, TContext2, TValue>); }

		IEvaluator<TContext2, TValue> IBridgeEvaluator<TContext1, TContext2, TValue>.evaluator { get => value; }

		protected override TValue OnEvaluate(TContext1 context) => value.Evaluate(Convert(context));

		public abstract TContext2 Convert(TContext1 context);

		public override string ToString() => value.ToString();
	}
}
