using System;
using System.Collections.Generic;
using Sapientia.Evaluators;
using Sapientia.Evaluators.Tracking;
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
	public abstract class BridgeCondition<TContext1, TContext2> : Condition<TContext1>,
		IBridgeEvaluator<TContext1, TContext2, bool>,
		ITrackableEvaluator
	{
		[SerializeReference]
		public Condition<TContext2> value;

		public IEvaluator Proxy => value;
		public Type ProxyType { get => typeof(Condition<TContext2>); }
		IEvaluator<TContext2, bool> IBridgeEvaluator<TContext1, TContext2, bool>.evaluator { get => value; }
		public Type TrackerType { get => typeof(BridgeEvaluatorTracker<TContext1, TContext2, bool>); }

		protected override bool OnEvaluate(TContext1 context) => value?.IsFulfilled(Convert(context)) ?? true;

		public abstract TContext2 Convert(TContext1 context);

		public override IEnumerator<IEvaluator> GetEnumerator()
		{
			yield return this;
		}
	}
}
