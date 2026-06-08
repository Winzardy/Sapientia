using System;
using Sapientia.Pooling;

namespace Sapientia
{
	internal interface IBlackboardToken : ISubscriptionToken
	{
		Type ValueType { get; }

		/// <param name="solo">
		/// Отписывать ли токен в <see cref="Blackboard"/>, мини хак чтобы при массовой отписке не дергать отписку отдельного токена
		/// </param>
		void Release(bool solo = true);

		internal IBlackboardToken Clone(Blackboard blackboard);
	}

	public class BlackboardToken<T> : IBlackboardToken, IPoolable
	{
		private int _generation;
		private BlackboardStorage<T>? _storage;
		private string? _key;
		private bool _suppressDispose;

		public Type ValueType { get => typeof(T); }

		int ISubscriptionToken.Generation { get => _generation; }

		internal Blackboard? Blackboard { get => _storage!.Blackboard; }
		internal string? Key { get => _key; }

		internal void Bind(BlackboardStorage<T> storage, string? key)
		{
			_storage = storage;
			_key = key;
			_suppressDispose = false;
		}

		public void Dispose()
		{
			if (_suppressDispose || _storage == null)
				return;

			Release(true);
		}

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
			if (!_suppressDispose)
				Clear();

			_suppressDispose = false;
		}

		internal void PreparePoolRelease()
		{
			Clear();
			_suppressDispose = true;
		}

		private void Clear()
		{
			_storage = null;
			_key = null;

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
