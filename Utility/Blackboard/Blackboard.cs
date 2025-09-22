using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains<T>(string? key = null) => Blackboard<T>.Contains(this, key);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get<T>(string? key = null) => Blackboard<T>.Get(this, key);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGet<T>(out T value) => TryGet(null, out value);

		public bool TryGet<T>(string? key, out T value)
		{
			if (!Contains<T>(key))
			{
				value = default;
				return false;
			}

			value = Get<T>(key);
			return true;
		}

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

		internal static T Get(Blackboard blackboard, string? key = null)
		{
			// CollectionsMarshal.GetValueRefOrNullRef
			var map = EnsureMap();
			if (map == null)
				throw GetException();

			var hash = ToHash(blackboard, key);
			if (!map.TryGetValue(hash, out var entry))
				throw GetException();

			return entry.value;

			Exception GetException()
			{
				var msg = $"{blackboard.GetName()}: ${typeof(T)} not found" +
					(!key.IsNullOrEmpty() ? $" by key [ {key} ]" : "");
				return blackboard.GetException(msg);
			}
		}

		internal static BlackboardToken<T> Register(in T value, Blackboard blackboard, string? key = null)
		{
			var map = EnsureMap();
			var hash = ToHash(blackboard, key);
			var newEntry = Pool<Entry>.Get();
			{
				newEntry.value = value;
			}
			if (!map.TryAdd(hash, newEntry))
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
		}

		private static ConcurrentDictionary<RegisteredTokenHash, Entry> EnsureMap()
		{
			var map = _boardToEntry;
			if (map != null)
				return map;

			var fresh = new ConcurrentDictionary<RegisteredTokenHash, Entry>();
			var raced = Interlocked.CompareExchange(ref _boardToEntry, fresh, null);
			return raced ?? fresh;
		}

		private static RegisteredTokenHash ToHash(Blackboard blackboard, string? key) => new(blackboard, key);

		// вместо CollectionsMarshal.GetValueRefOrNullRef... которого нет в Unity
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
