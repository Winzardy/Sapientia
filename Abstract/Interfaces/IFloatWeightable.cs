using Sapientia.Evaluators;

namespace Sapientia
{
	//TODO: сделать полоску отображения весов перед коллекцией<IFloatWeightable>
	public interface IFloatWeightable
	{
		public float Weight { get; }
	}

	public interface IFloatWeightableWithEvaluator<TContext>
	{
		public EvaluatedValue<TContext, float> Weight { get; }
	}
}
