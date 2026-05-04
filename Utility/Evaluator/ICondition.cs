
namespace Sapientia
{
	public interface ICondition
	{
		const float R = 0.5f;
		const float G = 0.5f;
		const float B = 1;
		const float A = 1;

		const string GROUP =  "condition";
		const int OPERATOR_WIDTH = 55;
	}

	public interface ICondition<in T> : IEvaluator<T, bool>, ICondition
	{
		bool IsFulfilled(T context);

		bool IEvaluator<T, bool>.Evaluate(T provider) => IsFulfilled(provider);
	}
}
