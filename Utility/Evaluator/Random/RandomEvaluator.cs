using System;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class RandomEvaluator<TContext, TValue> : Evaluator<TContext, TValue>, IRandomEvaluator<TValue>
		where TValue : struct, IComparable<TValue>

	{
		protected sealed override TValue OnEvaluate(TContext context)
		{
			var randomizer = GetRandomizer(context);
			return OnRandom(context, randomizer);
		}

		protected abstract TValue OnRandom(TContext context, IRandomizer<TValue> randomizer);

		protected abstract IRandomizer<TValue> GetRandomizer(TContext context);
	}
}
