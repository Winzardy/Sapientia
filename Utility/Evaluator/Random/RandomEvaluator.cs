using System;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class RandomEvaluator<TContext,TValue> : Evaluator<TContext,TValue>, IRandomEvaluator<TValue>
		where TValue : struct, IComparable<TValue>
	{
		public TValue min;
		public TValue max;

		public RandomEvaluator(TValue min, TValue max)
		{
			this.min = min;
			this.max = max;
		}

		protected sealed override TValue OnGet(TContext context)
		{
			var randomizer = GetRandomizer(context);
			return randomizer.Next(min, max);
		}

		public override string ToString() => $"{min}-{max}";

		protected abstract IRandomizer<TValue> GetRandomizer(TContext context);
	}
}
