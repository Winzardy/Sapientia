using System;

namespace Sapientia
{
	public partial class Blackboard
	{
		private bool _dummyMode;
		private int _dummyRequest;

		/// <summary>
		/// Режим симуляции покупки, чтобы вернуть стейт игры обратно (в нашем случае рандом)
		/// </summary>
		/// <remarks>
		/// Назвал Dummy вместо Fake потому что Fake конфликтует с Fetching
		/// </remarks>
		public bool DummyMode => _dummyMode;

		public event Action<bool> DummyModeChanged;

		protected internal void SetDummyMode(bool value)
		{
			if (!_active)
				throw new ArgumentException("Cannot change DummyMode for an inactive blackboard...");

			var prev = _dummyMode;

			if (value)
				_dummyRequest++;
			else
				_dummyRequest--;

			_dummyMode = _dummyRequest != 0;

			if (_dummyRequest < 0)
				throw new ArgumentException("DummyRequest cannot be less than zero...");

			if (prev != _dummyMode)
				DummyModeChanged?.Invoke(value);
		}

		private void OnReleaseDummyMode()
		{
			_dummyMode = false;
			_dummyRequest = 0;
		}

		public DummyScope DummyModeScope(bool value = true) => new(this, value);

		public readonly struct DummyScope : IDisposable
		{
			private readonly Blackboard _blackboard;

			public DummyScope(Blackboard blackboard, bool value = true)
			{
				_blackboard = blackboard;
				_blackboard.SetDummyMode(value);
			}

			public void Dispose()
			{
				_blackboard.SetDummyMode(false);
			}
		}
	}
}
