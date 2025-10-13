using System;
using UnityEngine;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class ProxyEvaluator<TContext1, TContext2, TValue> : Evaluator<TContext1, TValue>, IProxyEvaluator
	{
		[SerializeReference]
		public Evaluator<TContext2, TValue> value;

		protected override TValue OnGet(TContext1 context) => value.Get(Convert(context));

		protected abstract TContext2 Convert(TContext1 context);

		public IEvaluator Proxy => value;
		public Type ProxyType => typeof(Evaluator<TContext2, TValue>);
	}
}
