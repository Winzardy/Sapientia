using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;
using Sapientia.Pooling;
using Sapientia.Pooling.Concurrent;

namespace Sapientia
{
	public abstract class Blackboard : IDisposable
	{
		private ConcurrentHashSet<IBlackboardToken>? _tokens;

		public void Dispose()
		{
			if (_tokens == null)
				return;

			foreach (var token in _tokens)
				token.Release(false);

			StaticObjectPoolUtility.ReleaseSafe(ref _tokens);
		}

		internal virtual string GetName() => GetType().Name;

		public bool Contains<T>(string? key = null) => Blackboard<T>.Contains(this, key);
		public ref readonly T Get<T>(string? key = null) => ref Blackboard<T>.Get(this, key);

		public IBlackboardToken Register<T>(in T value, string? key = null)
		{
			_tokens ??= ConcurrentHashSetPool<IBlackboardToken>.Get();
			var token = Blackboard<T>.Register(in value, this, key);
			_tokens.Add(token);
			return token;
		}

		internal void Unregister(IBlackboardToken token)
		{
			if (_tokens == null || !_tokens.Remove(token))
				throw new ArgumentException($"[{GetName()}] {token.ValueType} not registered");
		}

		internal void Clone(Blackboard target)
		{
			if (_tokens == null)
				return;

			target._tokens ??= ConcurrentHashSetPool<IBlackboardToken>.Get();

			foreach (var token in _tokens)
			{
				var newToken = token.Clone(target);
				target._tokens.Add(newToken);
			}
		}
	}

	internal static class Blackboard<T>
	{
		private static ConcurrentDictionary<int, Entry>? _boardToValue;

		internal static bool Contains(Blackboard blackboard, string? key = null)
		{
			var hash = ToHash(blackboard, key);
			return _boardToValue != null && _boardToValue.ContainsKey(hash);
		}

		internal static ref readonly T Get(Blackboard blackboard, string? key = null)
		{
			// TODO: .NET 5+
			// ref readonly var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, key);
			// return ref entry.value;

			var hash = ToHash(blackboard, key);
			if (_boardToValue == null || !_boardToValue.TryGetValue(hash, out var entry))
				throw new ArgumentException($"[{blackboard.GetName()}] {typeof(T)} not found" +
					(!key.IsNullOrEmpty() ? $" by key '{key}'" : ""));

			return ref entry.value;
		}

		internal static IBlackboardToken Register(in T value, Blackboard blackboard, string? key = null)
		{
			_boardToValue ??= ConcurrentDictionaryPool<int, Entry>.Get();
			var hash = ToHash(blackboard, key);
			if (!_boardToValue.TryAdd(hash, value))
				throw new ArgumentException($"[{blackboard.GetName()}] {typeof(T)} already registered" +
					(!key.IsNullOrEmpty() ? $" with key '{key}'" : ""));

			return new BlackboardToken<T>(blackboard, key);
		}

		internal static void Unregister(Blackboard blackboard, string? key = null)
		{
			var hash = ToHash(blackboard, key);
			if (_boardToValue == null || !_boardToValue.TryRemove(hash, out _))
				throw new ArgumentException($"[{blackboard.GetName()}] {typeof(T)} not registered" +
					(!key.IsNullOrEmpty() ? $" by key '{key}'" : ""));

			if (_boardToValue.Count <= 0)
				StaticObjectPoolUtility.Release(ref _boardToValue);
		}

		private static int ToHash(Blackboard blackboard, string? key) =>
			key.IsNullOrEmpty() ? RuntimeHelpers.GetHashCode(blackboard) : HashCode.Combine(blackboard, key);

		internal class Entry
		{
			internal readonly T value;
			private Entry(in T value) => this.value = value;

			public static implicit operator Entry(in T value) => new(value);
		}
	}
}
