using System;
using System.Runtime.CompilerServices;
using Sapientia.Pooling;

namespace Sapientia
{
	internal interface IBlackboardToken : IDisposable
	{
		public Type ValueType { get; }

		/// <param name="solo">
		/// Отписывать ли токен в <see cref="Blackboard"/>, мини хак чтобы при массовой отписке не дергать отписку отдельного токена
		/// </param>
		public void Release(bool solo = true);

		internal int Generation { get; }
		internal IBlackboardToken Clone(Blackboard blackboard);
	}

	public class BlackboardToken<T> : IBlackboardToken, IPoolable
	{
		private int _generation;
		private BlackboardStorage<T>? _storage;
		private string? _key;

		public Type ValueType => typeof(T);

		int IBlackboardToken.Generation => _generation;

		internal Blackboard? Blackboard => _storage!.Blackboard;
		internal string? Key => _key;

		internal void Bind(BlackboardStorage<T> storage, string? key)
		{
			_storage = storage;
			_key     = key;
		}

		public void Dispose() => Release(true);

		/// <inheritdoc/>
		public void Release(bool solo)
		{
			if (_storage == null)
				throw new InvalidOperationException($"{typeof(T).Name} Token already released");

			if (solo)
				Blackboard!.ReleaseToken(this);

			_storage.Unregister(this);
		}

		void IPoolable.Release()
		{
			_storage = null;
			_key     = null;

			_generation++;
		}

		IBlackboardToken IBlackboardToken.Clone(Blackboard blackboard)
		{
			var value = _storage!.Get(_key);
			return blackboard.Register(in value, _key).Token;
		}

		public static implicit operator BlackboardToken(BlackboardToken<T> token) => new(token, token._generation);
	}
}
