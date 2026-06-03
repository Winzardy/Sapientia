using System;
using System.Collections.Generic;
using Sapientia.Utility;

namespace Sapientia.Evaluators.Tracking
{
	/// <summary>
	/// Резолвер трекер центра для конкретного типа контекста.
	/// Предоставляет способ извлечения <see cref="IEvaluatorTrackingCenter{TContext}"/>
	/// из произвольного контекста без требования явной зависимости от трекера.
	/// <br/><br/>
	/// Используется как связующее звено между системой Evaluator'ов и внешними
	/// контекстами, позволяя внедрять реактивное отслеживание без изменения
	/// самих контекстов.
	/// <br/><br/>
	/// Для каждого используемого типа контекста должен существовать ровно один
	/// зарегистрированный резолвер.
	/// <br/><br/>
	/// ⚠️ Отсутствие резолвера для типа контекста приведёт к ошибке при попытке подписки
	/// </summary>
	public interface IEvaluatorTrackingCenterResolver
	{
		Type ContextType { get; }
	}

	public abstract class EvaluatorTrackingCenterResolver<TContext> : IEvaluatorTrackingCenterResolver
	{
		public Type ContextType { get => typeof(TContext); }
		protected internal abstract IEvaluatorTrackingCenter<TContext> ResolveCenter(TContext context);
	}

	public static class EvaluatorTrackerResolverRegistry
	{
		private static Dictionary<Type, IEvaluatorTrackingCenterResolver>? _contextTypeToResolver;

		static EvaluatorTrackerResolverRegistry()
		{
			_contextTypeToResolver = new Dictionary<Type, IEvaluatorTrackingCenterResolver>();
			foreach (var resolver in ReflectionUtility.InstantiateAllTypes<IEvaluatorTrackingCenterResolver>())
				_contextTypeToResolver[resolver.ContextType] = resolver;
		}

		public static EvaluatorTrackingCenterResolver<TContext> GetResolverByContext<TContext>()
		{
			if (!_contextTypeToResolver!.TryGetValue(typeof(TContext), out var rawResolver))
				throw new InvalidOperationException($"No tracker registered for context type [ {typeof(TContext)} ]");

			if (rawResolver is not EvaluatorTrackingCenterResolver<TContext> resolver)
				throw new InvalidOperationException($"Registered tracker by type [ {rawResolver.GetType()} ] does not match expected " +
					$"type [ {typeof(EvaluatorTrackingCenterResolver<TContext>)} ] for context [ {typeof(TContext)}] ");

			return resolver;
		}
	}
}
