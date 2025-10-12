using System;
using Sapientia.Deterministic;

namespace Sapientia.Evaluators
{
	[Serializable]
	public abstract class Fix64RandomEvaluator<TContext> : RandomEvaluator<TContext, Fix64>
	{
		public Fix64RandomEvaluator() : this(Fix64.Zero, Fix64.One)
		{
		}

		public Fix64RandomEvaluator(Fix64 min, Fix64 max) : base(min, max)
		{
		}
	}
}
