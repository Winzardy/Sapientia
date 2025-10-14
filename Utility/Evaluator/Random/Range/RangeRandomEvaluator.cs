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
		public EvaluatedValue<TContext, TValue> min;
		public EvaluatedValue<TContext, TValue> max;

		public RangeRandomEvaluator(TValue min, TValue max)
		{
			this.min = min;
			this.max = max;
		}

		protected override TValue OnRandom(TContext context, IRandomizer<TValue> randomizer)
		{
			var minInclusive = min.Get(context);
			var maxExclusive = max.Get(context);
			return randomizer.Next(minInclusive, maxExclusive);
		}

		public override string ToString() => $"{min}-{max}";

#if CLIENT
		public const string SELECTOR_NAME = "\u2009Range (random)";
		public const string SELECTOR_CATEGORY = "/";
		public const Sirenix.OdinInspector.SdfIconType SELECTOR_ICON = Sirenix.OdinInspector.SdfIconType.DiamondHalf;
#endif
	}
}
