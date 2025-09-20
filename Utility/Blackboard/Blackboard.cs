using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;
using Sapientia.Pooling;
using Sapientia.Pooling.Concurrent;

namespace Sapientia
{
	/// <summary>
	/// Blackboard — типобезопасное runtime-хранилище значений по типу <typeparamref name="T"/> и необязательному ключу.
	/// Используется как общий контекст для обмена данными между подсистемами без прямых зависимостей
	/// </summary>
	/// <remarks>
	/// ⚠️ Важно: only-runtime
	/// </remarks>
	public abstract class Blackboard : IPoolable, IDisposable
	{
		private ConcurrentHashSet<IBlackboardToken>? _tokens;

		protected Blackboard(Blackboard? source = null)
		{
			if (source?._tokens == null)
				return;

			_tokens ??= ConcurrentHashSetPool<IBlackboardToken>.Get();

			foreach (var token in source._tokens)
			{
				var newToken = token.Clone(this);
				_tokens.Add(newToken);
			}
		}

		public void Dispose()
		{
			ReleaseInternal();
			OnDispose();
		}

		public bool Contains<T>(string? key = null) => Blackboard<T>.Contains(this, key);

		public ref readonly T Get<T>(string? key = null) => ref Blackboard<T>.Get(this, key);

		public BlackboardToken Register<T>(in T value, string? key = null)
		{
			_tokens ??= ConcurrentHashSetPool<IBlackboardToken>.Get();
			var token = Blackboard<T>.Register(in value, this, key);
			_tokens.Add(token);
			return token;
		}

		protected virtual void OnRelease()
		{
		}

		protected virtual void OnDispose()
		{
		}

		internal void ReleaseToken(IBlackboardToken token)
		{
			if (_tokens != null && _tokens.Remove(token))
				return;

			var msg = $"{Name}: ${token.ValueType} not registered";
			throw GetArgumentException(msg);
		}

		void IPoolable.Release() => ReleaseInternal();

		private void ReleaseInternal()
		{
			if (_tokens != null)
			{
				foreach (var token in _tokens)
					token.Release(false);

				StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _tokens);
			}

			OnRelease();
		}

		protected virtual string Name => GetType().Name;
		protected virtual Exception GetArgumentException(object msg) => new ArgumentException(msg.ToString());

		internal string GetName() => Name;
		internal Exception GetException(object msg) => GetArgumentException(msg);

		public sealed override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

		public static implicit operator bool(Blackboard? bb) => bb != null;
	}

	internal static class Blackboard<T>
	{
		private static ConcurrentDictionary<RegisteredTokenHash, Entry>? _boardToEntry;

		internal static bool Contains(Blackboard blackboard, string? key = null)
		{
			var hash = ToHash(blackboard, key);
			return _boardToEntry != null && _boardToEntry.ContainsKey(hash);
		}

		internal static ref readonly T Get(Blackboard blackboard, string? key = null)
		{
			// TODO: .NET 5+
			// ref readonly var entry = ref CollectionsMarshal.GetValueRefOrNullRef(_dict, key);
			// return ref entry.value;

			var hash = ToHash(blackboard, key);
			if (_boardToEntry == null || !_boardToEntry.TryGetValue(hash, out var entry))
			{
				var msg = $"{blackboard.GetName()}: ${typeof(T)} not found" +
					(!key.IsNullOrEmpty() ? $" by key [ {key} ]" : "");
				throw blackboard.GetException(msg);
			}

			return ref entry.value;
		}

		internal static BlackboardToken<T> Register(in T value, Blackboard blackboard, string? key = null)
		{
			_boardToEntry ??= ConcurrentDictionaryPool<RegisteredTokenHash, Entry>.Get();
			var hash = ToHash(blackboard, key);
			var newEntry = Pool<Entry>.Get();
			{
				newEntry.value = value;
			}
			if (!_boardToEntry.TryAdd(hash, newEntry))
			{
				Pool<Entry>.Release(newEntry);

				var msg = $"{blackboard.GetName()}: {typeof(T)} already registered" +
					(!key.IsNullOrEmpty() ? $" with key [ {key} ]" : "");
				throw blackboard.GetException(msg);
			}

			var token = Pool<BlackboardToken<T>>.Get();
			token.Bind(in hash);
			return token;
		}

		internal static void Unregister(BlackboardToken<T> token)
		{
			if (_boardToEntry == null || !_boardToEntry.TryRemove(token.Hash, out var entry))
			{
				var blackboard = token.Blackboard;
				var key = token.Key;
				var msg = $"{blackboard.GetName()}: {typeof(T)} not registered" +
					(!key.IsNullOrEmpty() ? $" by key [ {key} ]" : "");
				throw blackboard.GetException(msg);
			}

			Pool<Entry>.Release(entry);
			Pool<BlackboardToken<T>>.Release(token);

			if (_boardToEntry.IsEmpty)
				StaticObjectPoolUtility.ReleaseAndSetNull(ref _boardToEntry);
		}

		private static RegisteredTokenHash ToHash(Blackboard blackboard, string? key) => new(blackboard, key);

		internal class Entry : IPoolable
		{
			internal T value;

			void IPoolable.Release()
				=> value = default;
		}
	}

	public readonly struct BlackboardToken : IDisposable
	{
		private readonly IBlackboardToken _token;
		private readonly int _generation;

		public bool IsValid => _token != null && _token.Generation == _generation;

		internal BlackboardToken(IBlackboardToken token, int generation)
		{
			_token = token;
			_generation = generation;
		}

		public void Dispose() => Release();

		public void Release()
		{
			if (_token.Generation != _generation)
				throw new InvalidOperationException(
					$"[{nameof(BlackboardToken)}] Invalid token (token gen:{_token.Generation} != gen: {_generation})");

			_token.Release();
		}

		public void ReleaseSafe()
		{
			if (!IsValid)
				return;

			Release();
		}

		public static void ReleaseAndSetNull(ref BlackboardToken? token)
		{
			token?.Release();
			token = null;
		}
	}
}
