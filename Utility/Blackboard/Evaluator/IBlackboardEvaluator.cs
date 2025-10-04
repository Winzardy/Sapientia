using Sapientia.Evaluator;

namespace Sapientia.BlackboardEvaluator
{
	public interface IBlackboardEvaluator<out T> : IEvaluator<Blackboard, T>
	{
	}

	public abstract class BlackboardEvaluator<T> : IBlackboardEvaluator<T>
	{
		public const float R = 0.6f;
		public const float G = 1f;
		public const float B = 0.6f;
		public const float A = 1;

		T IEvaluator<Blackboard, T>.Evaluate(Blackboard blackboard) => Get(blackboard);

		public T Get(Blackboard blackboard) => OnGet(blackboard);

		protected abstract T OnGet(Blackboard blackboard);

		public static implicit operator BlackboardEvaluator<T>(T value) => BlackboardConstantFactory.Create<T>(value);
	}
}
