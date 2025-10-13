using System;

namespace Sapientia.Evaluators
{
	public interface IRangeEvaluator
	{
	}

	[Serializable]
	public abstract class RangeRandomEvaluator<TContext, TValue> : RandomEvaluator<TContext, TValue>, IRangeEvaluator
		where TValue : struct, IComparable<TValue>
	{
		public TValue min;
		public TValue max;

		public RangeRandomEvaluator(TValue min, TValue max)
		{
			this.min = min;
			this.max = max;
		}

		protected override TValue OnRandom(TContext context, IRandomizer<TValue> randomizer)
		{
			return randomizer.Next(min, max);
		}

		public override string ToString() => $"{min}-{max}";
	}
}
