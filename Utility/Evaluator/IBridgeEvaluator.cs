using System;

namespace Sapientia.Evaluators
{
	public interface IBridgeEvaluator : IEvaluator
	{
		IEvaluator Proxy { get; }
		Type ProxyType { get; }

		Type ContextType { get; }
		Type BridgeContextType { get; }
	}

	public interface IBridgeEvaluator<TContext> : IBridgeEvaluator
	{
		Type IBridgeEvaluator.ContextType { get => typeof(TContext); }
	}

	public interface IBridgeEvaluator<TContext1, TContext2, TValue> : IBridgeEvaluator<TContext1>
	{
		Type IBridgeEvaluator.BridgeContextType { get => typeof(TContext2); }
		TContext2 Convert(TContext1 context);
		internal IEvaluator<TContext2, TValue> evaluator { get; }
	}
}
