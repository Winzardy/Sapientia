using System;
using UnityEngine;

namespace Sapientia.Evaluators
{
	/// <summary>
	/// Обертка над <see cref="Evaluator{TContext, TValue}"/>, которая позволяет дешево
	/// хранить константное значение
	/// </summary>
	[Serializable]
	public partial struct EvaluatedValue<TContext, TValue>
	{
		public TValue value;

		[SerializeReference]
		public Evaluator<TContext, TValue> evaluator;

		public static implicit operator EvaluatedValue<TContext, TValue>(TValue value)
			=> new() {value = value};

		public static implicit operator EvaluatedValue<TContext, TValue>(Evaluator<TContext, TValue> evaluator)
			=> new() {evaluator = evaluator};

		public TValue Get(TContext context) => evaluator != null ? evaluator.Get(context) : value;
	}
}
