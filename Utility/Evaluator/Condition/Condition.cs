using System;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract partial class Condition<T> : ICondition<T>
	{
		bool IEvaluator<T, bool>.Evaluate(T context)
			=> IsFulfilled(context);

		public virtual bool IsFulfilled(T context) =>
			OnEvaluate(context);

		protected internal bool Evaluate(T context) => IsFulfilled(context);

		protected abstract bool OnEvaluate(T context);
	}
}
