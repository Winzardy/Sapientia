using System;
using System.Runtime.CompilerServices;

namespace Sapientia
{
	public interface IBlackboardToken : IDisposable
	{
		public Type ValueType { get; }

		/// <param name="unregister">
		/// Отписывать ли токен в <see cref="Blackboard"/>, нужно чтобы массово отписывать токены
		/// </param>
		public void Release(bool unregister = true);

		internal IBlackboardToken Clone(Blackboard blackboard);
	}

	internal class BlackboardToken<T> : IBlackboardToken
	{
		private bool _binded;

		private RegisteredTokenHash _hash;

		public Type ValueType => typeof(T);

		internal void Bind(in RegisteredTokenHash hash)
		{
			_hash = hash;
			_binded = true;
		}

		public void Dispose()
		{
			Release(true);
		}

		/// <inheritdoc/>
		public void Release(bool unregister)
		{
			if (!_binded)
				return;

			if (unregister)
				_hash.blackboard.Unregister(this);

			Blackboard<T>.Unregister(this);
		}

		internal void Clear()
		{
			_hash = default;
			_binded = false;
		}

		public static implicit operator RegisteredTokenHash(BlackboardToken<T> token) => token._hash;
		public static implicit operator Blackboard(BlackboardToken<T> token) => token._hash.blackboard;
		public static implicit operator string?(BlackboardToken<T> token) => token._hash.key;

		IBlackboardToken IBlackboardToken.Clone(Blackboard blackboard)
		{
			ref readonly var value = ref Blackboard<T>.Get(_hash.blackboard);
			return Blackboard<T>.Register(in value, blackboard, _hash.key);
		}
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
