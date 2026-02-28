using System;
using Sapientia.Evaluators;

namespace Sapientia
{
	//TODO: сделать полоску отображения весов перед коллекцией<IWeightable>
	public interface IWeightable
	{
		int Weight { get; }
	}

	public interface IWeightableWithEvaluator : IWeightable
	{
		Type ContextType { get; }
	}

	public interface IWeightableWithEvaluator<TContext> : IWeightableWithEvaluator
	{
		Type IWeightableWithEvaluator.ContextType { get => typeof(TContext); }

		EvaluatedValue<TContext, int> Weight { get; }
	}
}
