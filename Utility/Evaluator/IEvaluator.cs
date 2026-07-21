#nullable disable
using System;
using System.Collections.Generic;

namespace Sapientia
{
	public interface IEvaluator : IEnumerable<IEvaluator>
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

	public static class EvaluatorExtensions
	{
		public static T FindFirst<T>(this IEvaluator evaluator) where T : class, IEvaluator
		{
			return FindFirst<T>(evaluator, new HashSet<IEvaluator>());
		}

		private static T FindFirst<T>(IEvaluator evaluator, HashSet<IEvaluator> visited) where T : class, IEvaluator
		{
			if (evaluator == null || !visited.Add(evaluator))
				return null;

			if (evaluator is T target)
				return target;

			foreach (var child in evaluator)
			{
				var result = FindFirst<T>(child, visited);
				if (result != null)
					return result;
			}

			return null;
		}
	}
}
