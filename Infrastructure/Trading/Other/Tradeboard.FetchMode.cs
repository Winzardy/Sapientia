#if CLIENT
using System;
using Sapientia;

namespace Trading
{
	public sealed partial class Tradeboard
	{
		private bool _fetchMode;
		private int _fetchRequest;

		/// <summary>
		/// Режим при котором мы получаем квитанции (чеки), так же автоматически включается Dummy (фейк) режим
		/// </summary>
		public bool IsFetchMode => _fetchMode;

		/// <inheritdoc cref="IsFetchMode"/>
		internal void SetFetchMode(bool value)
		{
			if (value)
				_fetchRequest++;
			else
				_fetchRequest--;

			_fetchMode = _fetchRequest != 0;
		}

		/// <inheritdoc cref="IsFetchMode"/>
		public FetchScope FetchModeScope(bool value = true) => new(this, value);

		private void OnReleaseFetchMode()
		{
			_fetchMode = false;
			_fetchRequest = 0;
		}

		public readonly struct FetchScope : IDisposable
		{
			private readonly Tradeboard _tradeboard;
			private readonly bool _value;

			public FetchScope(Tradeboard tradeboard, bool value = true)
			{
				_value = value;
				_tradeboard = tradeboard;

				if (!value)
					return;

				_tradeboard.SetFetchMode(value);
				_tradeboard.SetSimulationMode(value);
			}

			public void Dispose()
			{
				if (!_value)
					return;

				_tradeboard.SetFetchMode(false);
				_tradeboard.SetSimulationMode(false);
			}
		}
	}
}
#endif
