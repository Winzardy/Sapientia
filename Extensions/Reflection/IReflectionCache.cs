#nullable enable

using Sapientia.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
namespace Sapientia.Utility
{
	public interface IReflectionCache
	{
		object? CreateInstance(Type? type);
		List<Type>? GetAllDerivedTypes(Type? baseType, Func<Type, bool>? predicate = null);
		Assembly[]? GetAssemblies() => null;
		Type[]? GetTypes(Assembly? assembly) => null;
	}

	public static class ReflectionCacheExtensions
	{
		public static T? CreateInstance<T>(this IReflectionCache cache, Type? type) where T : new()
		{
			var instance = cache.CreateInstance(type);
			return instance != null ? (T)instance : default;
		}

		public static bool TryCreateInstance<T>(this IReflectionCache cache, Type? type, [MaybeNullWhen(false)] out T instance) where T : new()
		{
			instance = cache.CreateInstance<T>(type);
			return instance != null;
		}

		public static bool TryCreateInstance(this IReflectionCache cache, Type? type, [MaybeNullWhen(false)] out object instance)
		{
			instance = cache.CreateInstance(type);
			return instance != null;
		}

		public static List<Type>? GetAllDerivedTypes<T>(this IReflectionCache cache, Func<Type, bool>? predicate = null)
		{
			return cache.GetAllDerivedTypes(typeof(T), predicate);
		}

		public static bool TryGetAllDerivedTypes<T>(this IReflectionCache cache, [NotNullWhen(true)] out List<Type>? types, Func<Type, bool>? predicate = null)
		{
			return cache.TryGetAllDerivedTypes(typeof(T), out types, predicate);
		}

		public static bool TryGetAllDerivedTypes(this IReflectionCache cache, Type? baseType, [NotNullWhen(true)] out List<Type>? types, Func<Type, bool>? predicate = null)
		{
			types = cache.GetAllDerivedTypes(baseType, predicate);
			return !types.IsNullOrEmpty();
		}
	}
}
