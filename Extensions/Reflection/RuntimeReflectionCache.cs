using Sapientia.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Sapientia.Utility
{
	public class RuntimeReflectionCache : IReflectionCache, IDisposable
	{
		private readonly string[] _allowedAssemblyTags;
		private readonly bool _trackAssemblyLoads;

		private readonly ConcurrentDictionary<Type, List<Type>> _derivedTypesCache = new ConcurrentDictionary<Type, List<Type>>();
		private readonly ConcurrentDictionary<Type, ConstructorInfo> _constructorsCache = new ConcurrentDictionary<Type, ConstructorInfo>();
		private readonly ConcurrentDictionary<Assembly, Type[]> _assemblyTypesCache = new ConcurrentDictionary<Assembly, Type[]>();

		private volatile Assembly[] _allowedAssemblies;
		private bool _subscribed;
		private bool _disposed;

		/// <summary>
		/// Set <paramref name="trackAssemblyLoads"/> only if assemblies can appear after the first query
		/// (Mono builds with runtime-loaded DLLs). Pointless under IL2CPP.
		/// Requires Dispose to unsubscribe.
		/// </summary>
		public RuntimeReflectionCache(string[] allowedAssemblyTags, bool trackAssemblyLoads = false)
		{
			_allowedAssemblyTags = allowedAssemblyTags ?? throw new ArgumentNullException(nameof(allowedAssemblyTags));
			_trackAssemblyLoads = trackAssemblyLoads;
		}

		public void Dispose()
		{
			if (_disposed)
				return;

			_disposed = true;

			if (_subscribed)
			{
				AppDomain.CurrentDomain.AssemblyLoad -= HandleAssemblyLoaded;
				_subscribed = false;
			}

			Invalidate();
		}

		public Assembly[] GetAssemblies()
		{
			var cached = _allowedAssemblies;
			if (cached != null)
				return cached;

			if (_trackAssemblyLoads && !_subscribed)
			{
				AppDomain.CurrentDomain.AssemblyLoad += HandleAssemblyLoaded;
				_subscribed = true;
			}

			var all = AppDomain.CurrentDomain.GetAssemblies();
			var list = new List<Assembly>(all.Length);

			for (int i = 0; i < all.Length; i++)
			{
				var assembly = all[i];
				if (IsAllowed(assembly))
				{
					list.Add(assembly);
				}
			}

			var built = list.ToArray();
			_allowedAssemblies = built;
			return built;
		}

		public Type[] GetTypes(Assembly assembly)
		{
			if (assembly == null)
				return Array.Empty<Type>();

			if (_assemblyTypesCache.TryGetValue(assembly, out var types))
				return types;

			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException e)
			{
#if UNITY_EDITOR
				UnityEngine.Debug.LogException(e);

				foreach (var loaderException in e.LoaderExceptions)
				{
					UnityEngine.Debug.LogException(loaderException);
				}
#endif
				types = SalvageTypes(e);
			}
			catch (Exception)
			{
				types = Array.Empty<Type>();
			}

			return _assemblyTypesCache.GetOrAdd(assembly, types);

			Type[] SalvageTypes(ReflectionTypeLoadException e)
			{
				if (e.Types == null)
					return Array.Empty<Type>();

				var loaded = new List<Type>(e.Types.Length);
				for (int i = 0; i < e.Types.Length; i++)
				{
					if (e.Types[i] != null)
					{
						loaded.Add(e.Types[i]);
					}
				}

				return loaded.ToArray();
			}
		}

		/// <summary>
		/// Call once on the main thread before any background work starts.
		/// Removes every write from the hot path, so later reads never contend.
		/// </summary>
		public void Warmup(params Type[] baseTypes)
		{
			var assemblies = GetAssemblies();

			for (int i = 0; i < assemblies.Length; i++)
			{
				GetTypes(assemblies[i]);
			}

			if (baseTypes.IsNullOrEmpty())
				return;

			for (int i = 0; i < baseTypes.Length; i++)
			{
				var baseType = baseTypes[i];
				if (baseType != null && !_derivedTypesCache.ContainsKey(baseType))
				{
					_derivedTypesCache[baseType] = BuildDerivedTypes(baseType);
				}
			}
		}

		/// <summary>
		/// Drops everything. Only needed if new assemblies can appear after the first query.
		/// </summary>
		public void Invalidate()
		{
			_allowedAssemblies = null;
			_assemblyTypesCache.Clear();
			_derivedTypesCache.Clear();
		}

		public object CreateInstance(Type type)
		{
			if (type == null ||
				type.IsAbstract ||
				type.IsInterface ||
				type.IsGenericTypeDefinition)
			{
				return null;
			}

			if (!_constructorsCache.TryGetValue(type, out var constructor))
			{
				constructor = type.GetConstructor(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
					null,
					Type.EmptyTypes,
					null);

				constructor = _constructorsCache.GetOrAdd(type, constructor);
			}

			return constructor != null ?
				constructor.Invoke(null) :
				Activator.CreateInstance(type);
		}

		public List<Type> GetAllDerivedTypes(Type baseType, Func<Type, bool> predicate = null)
		{
			if (baseType == null)
				return null;

			if (!_derivedTypesCache.TryGetValue(baseType, out var derivedTypes))
			{
				derivedTypes = _derivedTypesCache.GetOrAdd(baseType, BuildDerivedTypes(baseType));
			}

			if (derivedTypes.Count == 0)
				return null;

			if (predicate == null)
				return derivedTypes;

			var filtered = new List<Type>(derivedTypes.Count);
			for (int i = 0; i < derivedTypes.Count; i++)
			{
				var nextType = derivedTypes[i];
				if (predicate.Invoke(nextType))
				{
					filtered.Add(nextType);
				}
			}

			return filtered.Count > 0 ? filtered : null;
		}

		public IEnumerable<Type> GetAllTypes()
		{
			var assemblies = GetAssemblies();
			for (int i = 0; i < assemblies.Length; i++)
			{
				var types = GetTypes(assemblies[i]);
				for (int j = 0; j < types.Length; j++)
				{
					yield return types[j];
				}
			}
		}

		private bool IsAllowed(Assembly assembly)
		{
			var assemblyName = assembly.FullName;

			for (int i = 0; i < _allowedAssemblyTags.Length; i++)
			{
				if (assemblyName.Contains(_allowedAssemblyTags[i]))
					return true;
			}

			return false;
		}

		private List<Type> BuildDerivedTypes(Type baseType)
		{
			var list = new List<Type>();
			var assemblies = GetAssemblies();

			for (int i = 0; i < assemblies.Length; i++)
			{
				var types = GetTypes(assemblies[i]);
				for (int j = 0; j < types.Length; j++)
				{
					var type = types[j];

					if (type.IsInterface || type.IsAbstract)
						continue;

					if (type != baseType && baseType.IsAssignableFrom(type))
					{
						list.Add(type);
					}
				}
			}

			return list;
		}

		private void HandleAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
		{
			// Only an allowed assembly can change any answer we've cached.
			if (IsAllowed(args.LoadedAssembly))
			{
				Invalidate();
			}
		}
	}
}
