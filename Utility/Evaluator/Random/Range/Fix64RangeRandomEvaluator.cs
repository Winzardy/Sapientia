using System;
using Sapientia.Deterministic;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class Fix64RangeRandomEvaluator<TContext> : RangeRandomEvaluator<TContext, Fix64>
	{
		public Fix64RangeRandomEvaluator() : this(Fix64.Zero, Fix64.One)
		{
		}

		public Fix64RangeRandomEvaluator(Fix64 min, Fix64 max) : base(min, max)
		{
		}
	}
}
