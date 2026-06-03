
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

	public interface ICondition<in TContext> : IEvaluator<TContext, bool>, ICondition
	{
		bool IsFulfilled(TContext context);

		bool IEvaluator<TContext, bool>.Evaluate(TContext provider) => IsFulfilled(provider);
	}
}
