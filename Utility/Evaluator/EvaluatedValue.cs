using System;
using UnityEngine;

namespace Sapientia.Evaluators
{
	/// <summary>
	/// Обертка над <see cref="Evaluator{TContext, TValue}"/>, которая позволяет дешево
	/// хранить константное значение
	/// </summary>
	[Serializable]
	public partial struct EvaluatedValue<TContext, TValue> : IContainer<TValue>
	{
		public TValue value;

		[SerializeReference]
		public Evaluator<TContext, TValue> evaluator;

		public static implicit operator EvaluatedValue<TContext, TValue>(TValue value)
			=> new() {value = value};

		public static implicit operator EvaluatedValue<TContext, TValue>(Evaluator<TContext, TValue> evaluator)
			=> new() {evaluator = evaluator};

		/// <inheritdoc cref="Evaluator{TContext, TValue}.Evaluate(TContext)"/>
		public readonly TValue Evaluate(TContext context) => evaluator ? evaluator.Evaluate(context) : value;

		public override string ToString()
		{
			if (evaluator)
				return evaluator.ToString();

			return value?.ToString() ?? string.Empty;
		}
	}
}
