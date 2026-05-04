using System;
using System.Collections.Generic;
using Sapientia.Utility;

namespace Sapientia.Evaluators.Tracking
{
	/// <summary>
	/// Резолвер трекера для конкретного типа контекста.
	/// Предоставляет способ извлечения <see cref="IEvaluatorTracker{TContext}"/>
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
	public interface IEvaluatorTrackerResolver
	{
	}

	public abstract class EvaluatorTrackerResolver<TContext> : IEvaluatorTrackerResolver
	{
		private EvaluatorTrackerResolver()
		{
		}

		protected internal abstract IEvaluatorTracker<TContext> GetTracker(TContext context);
	}

	public static class EvaluatorTrackerResolverRegistry
	{
		private static Dictionary<Type, IEvaluatorTrackerResolver>? _contextTypeToResolver;

		public static EvaluatorTrackerResolver<TContext> GetResolverByContext<TContext>()
		{
			if (_contextTypeToResolver == null)
				Fill();

			if (!_contextTypeToResolver!.TryGetValue(typeof(TContext), out var rawResolver))
				throw new InvalidOperationException($"No tracker registered for context type [ {typeof(TContext)} ]");

			if (rawResolver is not EvaluatorTrackerResolver<TContext> resolver)
				throw new InvalidOperationException($"Registered tracker by type [ {rawResolver.GetType()} ] does not match expected " +
					$"type [ {typeof(EvaluatorTrackerResolver<TContext>)} ] for context [ {typeof(TContext)}] ");

			return resolver;
		}

		private static void Fill()
		{
			_contextTypeToResolver = ReflectionUtility.InstantiateAllTypesMap<IEvaluatorTrackerResolver>();
		}
	}
}
