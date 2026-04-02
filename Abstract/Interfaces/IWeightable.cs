using System;
using Sapientia.Evaluators;

namespace Sapientia
{
	//TODO: сделать полоску отображения весов перед коллекцией<IWeightable>
	public interface IWeightable
	{
		int Weight { get; }
	}

	public interface IWeightableWithEvaluator : IWeightable // Наследование от IWeightable хак для редактора
	{
		Type ContextType { get; }
	}

	public interface IWeightable<TContext> : IWeightableWithEvaluator
	{
		Type IWeightableWithEvaluator.ContextType { get => typeof(TContext); }

		EvaluatedValue<TContext, int> EvaluatedWeight { get; }
	}
}
