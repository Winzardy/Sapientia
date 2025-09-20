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

	internal class BlackboardToken<T> : IBlackboardToken, IPoolable
	{
		private int _generation;
		private RegisteredTokenHash _hash;

		public Type ValueType => typeof(T);

		int IBlackboardToken.Generation => _generation;

		internal void Bind(in RegisteredTokenHash hash)
		{
			_hash = hash;
		}

		public void Dispose() => Release(true);

		/// <inheritdoc/>
		public void Release(bool solo)
		{
			if (_hash == default)
				throw new InvalidOperationException("Token already released");

			if (solo)
				_hash.blackboard.ReleaseToken(this);

			Blackboard<T>.Unregister(this);
		}

		internal ref readonly RegisteredTokenHash Hash => ref _hash;
		internal Blackboard Blackboard => _hash.blackboard;
		internal string? Key => _hash.key;

		void IPoolable.Release()
		{
			_hash = default;
			_generation++;
		}

		IBlackboardToken IBlackboardToken.Clone(Blackboard blackboard)
		{
			ref readonly var value = ref Blackboard<T>.Get(_hash.blackboard);
			return Blackboard<T>.Register(in value, blackboard, _hash.key);
		}

		public static implicit operator BlackboardToken(BlackboardToken<T> token) => new(token, token._generation);
	}

	/// <summary>
	/// Уникальный идентификатор записи в Blackboard, составленный из ссылки на Blackboard и строкового ключа
	/// </summary>
	internal readonly struct RegisteredTokenHash : IEquatable<RegisteredTokenHash>
	{
		internal readonly Blackboard blackboard;
		internal readonly string? key;

		internal RegisteredTokenHash(Blackboard blackboard, string? key)
		{
			this.blackboard = blackboard;
			this.key = key;
		}

		public bool Equals(RegisteredTokenHash other)
		{
			return ReferenceEquals(blackboard, other.blackboard)
				&& string.Equals(key, other.key, StringComparison.Ordinal);
		}

		public override bool Equals(object? obj) => obj is RegisteredTokenHash other && Equals(other);

		public override int GetHashCode()
		{
			var h = RuntimeHelpers.GetHashCode(blackboard);
			if (key != null)
			{
				unchecked
				{
					h = (h * 397) ^ StringComparer.Ordinal.GetHashCode(key);
				}
			}

			return h;
		}

		public static bool operator ==(RegisteredTokenHash left, RegisteredTokenHash right) => left.Equals(right);
		public static bool operator !=(RegisteredTokenHash left, RegisteredTokenHash right) => !left.Equals(right);
	}
}
