using System;

namespace Sapientia.Evaluator.Blackboard
{
	[Serializable]
	public class BlackboardRandomEvaluator<T> : BlackboardEvaluator<T>, IRandomEvaluator<T>
		where T : struct, IComparable<T>
	{
		public T min;
		public T max;

		public BlackboardRandomEvaluator(T min, T max)
		{
			this.min = min;
			this.max = max;
		}

		protected sealed override T OnGet(Sapientia.Blackboard blackboard)
		{
			var randomizer = blackboard.Get<IRandomizer<T>>();
			return randomizer.Next(min, max);
		}

		public override string ToString() => $"{min}-{max}";
	}
}
