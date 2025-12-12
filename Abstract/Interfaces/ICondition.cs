
namespace Sapientia
{
	public interface ICondition
	{
		public const float R = 0.5f;
		public const float G = 0.5f;
		public const float B = 1;
		public const float A = 1;

		public const string GROUP =  "condition";
		public const int OPERATOR_WIDTH = 55;
	}

	public interface ICondition<in T> : IEvaluator<T, bool>, ICondition
	{
		public bool IsFulfilled(T context);

		bool IEvaluator<T, bool>.Evaluate(T provider) => IsFulfilled(provider);
	}
}
