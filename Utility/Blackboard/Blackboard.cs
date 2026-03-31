using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Pooling;

namespace Sapientia
{
	/// <summary>
	/// Blackboard — типобезопасное runtime-хранилище значений по типу <typeparamref name="T"/> и необязательному ключу.
	/// Используется как общий контекст для обмена данными между подсистемами без прямых зависимостей
	/// </summary>
	/// <remarks>
	/// ⚠️ Важно: only-runtime
	/// </remarks>
	public partial class Blackboard : IPoolable, IDisposable
	{
		private HashSet<IBlackboardToken>? _tokens;
		private Dictionary<Type, IBlackboardStorage> _typeToStorage;

		[InvokeOnceEvent]
		public event Action? Released;

		protected internal bool _active = true;

		public Blackboard()
		{
			_typeToStorage = new Dictionary<Type, IBlackboardStorage>();
		}

		public Blackboard(Blackboard? source = null) : this()
		{
			if (source?._tokens == null)
				return;

			_tokens ??= HashSetPool<IBlackboardToken>.Get();

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

		private void ReleaseInternal()
		{
			_active = false;

			if (_tokens != null)
			{
				foreach (var token in _tokens)
					token.Release(false);

				StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _tokens);
			}

			OnReleaseSimulationMode();

			foreach (var storage in _typeToStorage.Values)
				storage.ReleaseToPool();

			_typeToStorage.Clear();

			Released?.Invoke();
			Released = null;

			OnRelease();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains<T>(string? key = null)
		{
			if (!_active)
			{
				var keyLabel = key != null ? $" ({key})" : string.Empty;
				throw new BlackboardException($"Attempt to get {typeof(T).Name}{keyLabel} from an inactive Blackboard");
			}

			return TryGetStorage<T>(out var storage) && storage!.Contains(key);
		}

		public bool Any<T>()
		{
			if (_tokens.IsNullOrEmpty())
				return false;

			foreach (var rawToken in _tokens)
			{
				if (rawToken is BlackboardToken<T> token)
					return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Get<T>(string? key = null)
		{
			if (!_active)
			{
				var keyLabel = key != null ? $" ({key})" : string.Empty;
				throw new BlackboardException($"Attempt to get {typeof(T).Name}{keyLabel} from an inactive Blackboard");
			}

			if (!TryGetStorage<T>(out var storage))
			{
				var keyLabel = key != null ? $" with key [{key}]" : string.Empty;
				throw new BlackboardException($"Attempt to get value of type '{typeof(T).Name}'{keyLabel}, but it is not registered");
			}

			return storage!.Get(key);
		}

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
			_tokens ??= HashSetPool<IBlackboardToken>.Get();

			var storage = GetOrCreateStorage<T>();
			var token = storage.Register(in value, key);
			_tokens.Add(token);
			return token;
		}

		public void Overwrite<T>(in T value, string? key = null)
		{
			if (!TryGetStorage<T>(out var storage))
			{
				var keyLabel = key != null ? $" with key [{key}]" : string.Empty;
				throw new BlackboardException($"Attempt to get value of type '{typeof(T).Name}'{keyLabel}, but it is not registered");
			}

			storage!.Overwrite(in value, key);
		}

		internal void ReleaseToken(IBlackboardToken token)
		{
			if (_tokens != null && _tokens.Remove(token))
				return;

			var msg = $"{Name}: ${token.ValueType} not registered";
			throw new BlackboardException(msg);
		}

		void IPoolable.OnGet()
		{
			_active = true;
		}

		void IPoolable.Release()
		{
			ReleaseInternal();
		}

		protected virtual void OnRelease()
		{
		}

		protected virtual void OnDispose()
		{
		}

		private bool TryGetStorage<T>(out IBlackboardStorage<T>? storage)
		{
			if (_typeToStorage.TryGetValue(typeof(T), out var rawStorage))
			{
				storage = (IBlackboardStorage<T>) rawStorage;
				return true;
			}

			storage = null;
			return false;
		}

		private IBlackboardStorage<T> GetOrCreateStorage<T>()
		{
			if (_typeToStorage.TryGetValue(typeof(T), out var rawStorage))
				return (IBlackboardStorage<T>) rawStorage;

			var newStorage = Pool<BlackboardStorage<T>>.Get();
			newStorage.Bind(this);
			_typeToStorage.Add(typeof(T), newStorage);
			return newStorage;
		}

		protected virtual string Name => GetType().Name;

		internal string GetName() => Name;

		public sealed override int GetHashCode() => RuntimeHelpers.GetHashCode(this);

		public static implicit operator bool(Blackboard? bb) => bb != null;
	}

	public class BlackboardException : ArgumentException
	{
		public BlackboardException(string msg) : base(msg)
		{
		}
	}

	public readonly struct BlackboardToken : IDisposable
	{
		private readonly IBlackboardToken _token;
		private readonly int _generation;

		public bool IsValid => _token != null && _token.Generation == _generation;
		internal IBlackboardToken Token { get => _token; }

		internal BlackboardToken(IBlackboardToken token, int generation)
		{
			_token      = token;
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
