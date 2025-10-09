using Sapientia.Evaluator;

namespace Sapientia.BlackboardEvaluator
{
	public interface IBlackboardEvaluator<out T> : IEvaluator<Blackboard, T>
	{
	}

	public abstract class BlackboardEvaluator<T> : IBlackboardEvaluator<T>
	{
		public const float R = IEvaluator.R;
		public const float G = IEvaluator.G;
		public const float B = IEvaluator.B;
		public const float A = IEvaluator.A;

		T IEvaluator<Blackboard, T>.Evaluate(Blackboard blackboard) => Get(blackboard);

		public T Get(Blackboard blackboard) => OnGet(blackboard);

		protected abstract T OnGet(Blackboard blackboard);

		public static implicit operator BlackboardEvaluator<T>(T value) => BlackboardConstantFactory.Create<T>(value);
	}

}
