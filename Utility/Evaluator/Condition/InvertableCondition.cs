using System;

namespace Sapientia.Conditions
{
	[Serializable]
	public abstract class InvertableCondition<T> : Condition<T>
	{
		public bool invert;

		public override bool IsFulfilled(T context) => invert
			? !base.IsFulfilled(context)
			: base.IsFulfilled(context);
	}
}
