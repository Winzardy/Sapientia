using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sapientia.Collections;

namespace Sapientia.Reflection
{
	public static class FastActivator
	{
		internal static readonly ConcurrentDictionary<Type, Func<object>> typeToFactory = new();
		internal static readonly ConcurrentDictionary<Type, Func<object[], object>> typeToFactoryWithArgs = new();

		public static bool TryCreateInstance<T>(this Type type, out T instance)
		{
			try
			{
				instance = type.CreateInstance<T>();
				return true;
			}
			catch (Exception e)
			{
#if UNITY_EDITOR
				UnityEngine.Debug.LogError(e.Message);
#endif
				instance = default;
				return false;
			}
		}

		/// <summary>
		/// Быстрее чем <see cref="Activator.CreateInstance{T}"/> в ~8 раз, но медленнее new() в ~2 раза
		/// </summary>
		public static T CreateInstance<T>() => FastActivatorFactory<T>.Create();

		public static object CreateInstance(Type type) => typeToFactory.GetOrAdd(type, CompileFactory);

		public static T CreateInstance<T>(params object[] args) => FastActivatorFactory<T>.Create(args);

		public static T CreateInstance<T>(this Type type, params object[] args)
		{
			if (args.IsNullOrEmpty())
			{
				var factory = typeToFactory.GetOrAdd(type, CompileFactory);
				return (T) factory();
			}

			var factoryWithArgs = typeToFactoryWithArgs.GetOrAdd(type, CompileFactoryWithArgs);
			return (T) factoryWithArgs(args);
		}

		private static Func<object> CompileFactory(Type type)
		{
			if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
			{
				var structLambda = Expression.Lambda<Func<object>>(Expression.Convert(Expression.Default(type), typeof(object)));
				return structLambda.Compile();
			}

			var ctor = type.GetConstructor(Type.EmptyTypes);
			if (ctor == null)
				throw new InvalidOperationException($"No parameterless constructor for {type}");
			var newExpression = Expression.New(ctor);
			var lambda = Expression.Lambda<Func<object>>(newExpression);
			return lambda.Compile();
		}

		private static Func<object[], object> CompileFactoryWithArgs(Type type)
		{
			var paramAmountToFactory = new ConcurrentDictionary<int, Func<object[], object>>();

			foreach (var ctor in GetAllowedCtors(type))
			{
				var argsParam = Expression.Parameter(typeof(object[]), "args");
				var ctorParams = ctor.GetParameters();

				var args = ctorParams.Select((param, index) =>
					Expression.Convert(
						Expression.ArrayIndex(argsParam, Expression.Constant(index)),
						param.ParameterType
					)).ToArray();

				var newExp = Expression.New(ctor, args);
				Expression body = type.IsValueType
					? Expression.Convert(newExp, typeof(object)) // box
					: newExp;
				var lambda = Expression.Lambda<Func<object[], object>>(body, argsParam);
				paramAmountToFactory[ctorParams.Length] = lambda.Compile();
			}

			return args =>
			{
				args ??= Array.Empty<object>();

				if (paramAmountToFactory.TryGetValue(args.Length, out var factoryWithArgs))
					return factoryWithArgs(args);

				throw new InvalidOperationException($"No matching constructor found for {type} with {args.Length} arguments");
			};
		}

		/// <remarks>
		/// Не поддерживаем конструкторы с <c>ref</c>, <c>in</c>
		/// </remarks>
		internal static ConstructorInfo[] GetAllowedCtors(Type type)
		{
			return type.GetConstructors()
			   .Where(c => c.GetParameters().All(p =>
					p.ParameterType is {IsByRef: false}))//, IsValueType: false}))
			   .ToArray();
		}
	}

	internal static class FastActivatorFactory<T>
	{
		private static readonly Func<T> _func = CompileFactory();
		private static readonly Func<object[], T> _funcWithArgs = CompileFactoryWithArgs();

		public static T Create() => _func();
		public static T Create(params object[] args) => _funcWithArgs(args);

		private static Func<T> CompileFactory()
		{
			var type = typeof(T);
			if (type.IsValueType && Nullable.GetUnderlyingType(type) == null)
			{
				var structLambda = Expression.Lambda<Func<T>>(Expression.Convert(Expression.Default(type), type));
				return structLambda.Compile();
			}

			var ctor = type.GetConstructor(Type.EmptyTypes);
			if (ctor == null)
				throw new InvalidOperationException($"No parameterless constructor for {typeof(T)}");
			var newExpression = Expression.New(ctor);
			var lambda = Expression.Lambda<Func<T>>(newExpression);
			FastActivator.typeToFactory[typeof(T)] = ToObject;
			return lambda.Compile();
		}

		private static Func<object[], T> CompileFactoryWithArgs()
		{
			var paramAmountToFactory = new ConcurrentDictionary<int, Func<object[], T>>();

			var type = typeof(T);
			foreach (var ctor in FastActivator.GetAllowedCtors(type))
			{
				var argsParam = Expression.Parameter(typeof(object[]), "args");
				var ctorParams = ctor.GetParameters();

				var args = ctorParams.Select((param, index) =>
					Expression.Convert(
						Expression.ArrayIndex(argsParam, Expression.Constant(index)),
						param.ParameterType
					)).ToArray();

				var newExp = Expression.New(ctor, args);
				Expression body = type.IsValueType
					? Expression.Convert(newExp, typeof(object))
					: newExp;
				var lambda = Expression.Lambda<Func<object[], T>>(newExp, argsParam);
				paramAmountToFactory[ctorParams.Length] = lambda.Compile();
			}

			FastActivator.typeToFactoryWithArgs[type] = ToObjectWithArgs;

			return args =>
			{
				args ??= Array.Empty<object>();

				if (paramAmountToFactory.TryGetValue(args.Length, out var factoryWithArgs))
					return factoryWithArgs(args);

				throw new InvalidOperationException($"No matching constructor found for {type} with {args.Length} arguments");
			};
		}

		private static object ToObject() => _func();
		private static object ToObjectWithArgs(object[] args) => _funcWithArgs(args);
	}
}
