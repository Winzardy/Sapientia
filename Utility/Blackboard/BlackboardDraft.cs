using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Sapientia
{
	public interface IEvaluator<in TContext, out T>
	{
		T Evaluate(TContext context);
	}

	public interface IBlackboardEvaluator<out T> : IEvaluator<Blackboard, T>
	{
	}

	public abstract class ConstantEvaluator<TContext, T> : IEvaluator<TContext, T>
	{
		public T value;

		public T Evaluate(TContext _) => value;
	}

	public static class StaticAddOps<T>
	{
		public static readonly Func<T, T, T> Add = BuildAdd();

		private static readonly ConcurrentDictionary<Type, object> _overrides = new();

		public static void Override<TOverride>(Func<TOverride, TOverride, TOverride> fn)
			where TOverride : T
			=> _overrides[typeof(TOverride)] = fn;

		private static Func<T, T, T> BuildAdd()
		{
			if (_overrides.TryGetValue(typeof(T), out var ov))
				return (Func<T, T, T>) ov;

			var a = Expression.Parameter(typeof(T), "a");
			var b = Expression.Parameter(typeof(T), "b");
			try
			{
				var body = Expression.Add(a, b);
				return Expression.Lambda<Func<T, T, T>>(body, a, b).Compile();
			}
			catch
			{
				return (_, _) => throw new NotSupportedException(
					$"Type '{typeof(T).Name}' doesn't support operator +. " +
					$"Register custom adder via Ops<{typeof(T).Name}>.RegisterAdd(...)");
			}
		}
	}

	[Serializable]
	public sealed class BlackboardAddNode<T> : IBlackboardEvaluator<T>
	{
#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		public IBlackboardEvaluator<T> a;

#if CLIENT
		[UnityEngine.SerializeReference]
#endif
		public IBlackboardEvaluator<T> b;

		public T Evaluate(Blackboard bb) => StaticAddOps<T>.Add(a.Evaluate(bb), b.Evaluate(bb));
	}

	[Serializable]
	public class BlackboardConstant<T> : ConstantEvaluator<Blackboard, T>, IBlackboardEvaluator<T>
	{
	}

	[Serializable]
	public class BlackboardValue<T> : IBlackboardEvaluator<T>
	{
		private const string CATALOG_ID = "BlackboardKeys";

		//	[ContextLabel(CATALOG_ID)]
		public Toggle<string> key;

		public T Evaluate(Blackboard context)
		{
			return key
				? context.Get<T>(key)
				: context.Get<T>();
		}
	}
}
