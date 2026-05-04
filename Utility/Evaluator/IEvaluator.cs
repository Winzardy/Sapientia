using System;

namespace Sapientia
{
	public interface IEvaluator
	{
		const float R = 0.6f;
		const float G = 1f;
		const float B = 0.6f;
		const float A = 1;

		const string ARITHMETIC_OPERATOR_ADD = "+";
		const string ARITHMETIC_OPERATOR_SUBTRACT = "\u2212";
		const string ARITHMETIC_OPERATOR_DIVIDE = "\u00f7";
		const string ARITHMETIC_OPERATOR_MULTIPLY = "\u00d7";
		const string ARITHMETIC_OPERATOR_MODULUS = "%";

		Type ContextType { get; }
	}

	public interface IEvaluator<in TContext, out T> : IEvaluator
	{
		T Evaluate(TContext context);
		Type IEvaluator.ContextType { get => typeof(TContext); }
	}
}
