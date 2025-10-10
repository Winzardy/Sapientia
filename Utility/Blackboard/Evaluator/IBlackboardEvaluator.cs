using Sapientia.Evaluator;

namespace Sapientia.Evaluator.Blackboard
{
	public interface IBlackboardEvaluator<out T> : IEvaluator<Sapientia.Blackboard, T>
	{
	}

	public abstract class BlackboardEvaluator<T> : IBlackboardEvaluator<T>
	{
		public const float R = IEvaluator.R;
		public const float G = IEvaluator.G;
		public const float B = IEvaluator.B;
		public const float A = IEvaluator.A;

		T IEvaluator<Sapientia.Blackboard, T>.Evaluate(Sapientia.Blackboard blackboard) => Get(blackboard);

		public T Get(Sapientia.Blackboard blackboard) => OnGet(blackboard);

		protected abstract T OnGet(Sapientia.Blackboard blackboard);

		public static implicit operator BlackboardEvaluator<T>(T value) => BlackboardConstantFactory.Create<T>(value);
	}

}
