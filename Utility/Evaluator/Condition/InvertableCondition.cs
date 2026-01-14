using System;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract class InvertableCondition<T> : Condition<T>
	{
		public bool invert;

		protected override bool EvaluateInternal(T context) => invert
			? !base.EvaluateInternal(context)
			: base.EvaluateInternal(context);
	}
}
