using System;

namespace Sapientia
{
	public interface IBlackboardToken : IDisposable
	{
		public Type ValueType { get; }

		/// <param name="unregister">
		/// Отписывать ли токен в <see cref="Blackboard"/>
		/// </param>
		public void Release(bool unregister);

		internal IBlackboardToken Clone(Blackboard blackboard);
	}

	internal class BlackboardToken<T> : IBlackboardToken
	{
		private readonly Blackboard _blackboard;
		private readonly string? _key;

		public Type ValueType => typeof(T);

		public BlackboardToken(Blackboard blackboard, string? key)
		{
			_blackboard = blackboard;
			_key = key;
		}

		public void Dispose()
		{
			Release(true);
		}

		/// <inheritdoc/>
		public void Release(bool unregister)
		{
			if (unregister)
				_blackboard.Unregister(this);

			Blackboard<T>.Unregister(_blackboard, _key);
		}

		IBlackboardToken IBlackboardToken.Clone(Blackboard blackboard)
		{
			ref readonly var value = ref Blackboard<T>.Get(_blackboard);
			return Blackboard<T>.Register(in value, blackboard, _key);
		}
	}
}
