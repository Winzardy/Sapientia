using Sapientia.Evaluators;

namespace Sapientia
{
	//TODO: сделать полоску отображения весов перед коллекцией<IWeightable>
	public interface IWeightable
	{
		public int Weight { get; }
	}

	public interface IWeightableWithEvaluator<TContext>
	{
		public Evaluator<TContext, int> Weight { get; }
	}
}
