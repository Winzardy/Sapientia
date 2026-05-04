using System;
using Sapientia.Evaluators;
using UnityEngine;

namespace Sapientia.Conditions
{
	/// <summary>
	/// Мост между двумя типами контекста:
	/// преобразует <typeparamref name="TContext1"/> в <typeparamref name="TContext2"/>
	/// и делегирует вычисление вложенному Condition.
	/// <br/><br/>
	/// Позволяет переиспользовать Condition'ы, определённые для одного контекста,
	/// в рамках другого без жёсткой зависимости между ними
	/// </summary>
	/// <typeparam name="TContext1">Исходный контекст</typeparam>
	/// <typeparam name="TContext2">Целевой контекст</typeparam>
	[Serializable]
	public abstract class BridgeCondition<TContext1, TContext2> : Condition<TContext1>, IBridgeEvaluator
	{
		[SerializeReference]
		public Condition<TContext2> value;

		protected override bool OnEvaluate(TContext1 context) => value?.IsFulfilled(Convert(context)) ?? true;

		protected abstract TContext2 Convert(TContext1 context);

		public IEvaluator Proxy => value;
		public Type ProxyType => typeof(Condition<TContext2>);
	}
}
