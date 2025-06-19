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
			{
				var msg = $"{Name}: ${token.ValueType} not registered";
				throw GetArgumentException(msg);
			}
		}

		protected virtual void OnRelease()
		{
		}

		protected virtual void OnDispose()
		{
		}

		void IPoolable.Release() => ReleaseInternal();

		private void ReleaseInternal()
		{
			if (_tokens != null)
			{
				foreach (var token in _tokens)
					token.Release(false);

				StaticObjectPoolUtility.ReleaseSafe(ref _tokens);
			}

			OnRelease();
		}

		protected virtual string Name => GetType().Name;
		protected virtual Exception GetArgumentException(object msg) => new ArgumentException(msg.ToString());

		internal string GetName() => Name;
		internal Exception GetException(object msg) => GetArgumentException(msg);
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
			{
				var msg = $"{blackboard.GetName()}: ${typeof(T)} not found" +
					(!key.IsNullOrEmpty() ? $" by key [ {key} ]" : "");
				throw blackboard.GetException(msg);
			}

			return ref entry.value;
		}

		internal static IBlackboardToken Register(in T value, Blackboard blackboard, string? key = null)
		{
			_boardToValue ??= ConcurrentDictionaryPool<int, Entry>.Get();
			var hash = ToHash(blackboard, key);
			if (!_boardToValue.TryAdd(hash, value))
			{
				var msg = $"{blackboard.GetName()}: ${typeof(T)} already registered" +
					(!key.IsNullOrEmpty() ? $" with key [ {key} ]" : "");
				throw blackboard.GetException(msg);
			}

			return new BlackboardToken<T>(blackboard, key);
		}

		internal static void Unregister(Blackboard blackboard, string? key = null)
		{
			var hash = ToHash(blackboard, key);
			if (_boardToValue == null || !_boardToValue.TryRemove(hash, out _))
			{
				var msg = $"{blackboard.GetName()}: ${typeof(T)} not registered" +
					(!key.IsNullOrEmpty() ? $" by key [ {key} ]" : "");
				throw blackboard.GetException(msg);
			}

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
