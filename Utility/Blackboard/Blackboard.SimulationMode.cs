#nullable disable
using System;

namespace Sapientia
{
	public partial class Blackboard
	{
		private bool _isSimulationMode;
		private int _simulationRequestCount;

		/// <summary>
		/// Режим симуляции покупки, чтобы вернуть стейт игры обратно (в нашем случае рандом)
		/// </summary>
		/// <remarks>
		/// Назвал Dummy вместо Fake потому что Fake конфликтует с Fetching
		/// </remarks>
		public bool IsSimulationMode => _isSimulationMode;

		/// <remarks>
		/// ⚠️ Автоочищается при Dispose, отписка не обязательна
		/// </remarks>
		public event Action<bool> SimulationModeChanged;

		protected internal void SetSimulationMode(bool value)
		{
			if (!_active)
				throw new ArgumentException("Cannot change simulation mode for an inactive blackboard...");

			var prev = _isSimulationMode;

			if (value)
				_simulationRequestCount++;
			else
				_simulationRequestCount--;

			_isSimulationMode = _simulationRequestCount != 0;

			if (_simulationRequestCount < 0)
				throw new ArgumentException("Simulation request count cannot be less than zero...");

			if (prev != _isSimulationMode)
				SimulationModeChanged?.Invoke(value);
		}

		private void OnReleaseSimulationMode()
		{
			_isSimulationMode = false;
			_simulationRequestCount = 0;

			SimulationModeChanged = null;
		}

		public SimulationScope SimulationModeScope(bool value = true) => new(this, value);

		public readonly struct SimulationScope : IDisposable
		{
			private readonly Blackboard _blackboard;

			public SimulationScope(Blackboard blackboard, bool value = true)
			{
				_blackboard = blackboard;
				_blackboard.SetSimulationMode(value);
			}

			public void Dispose()
			{
				_blackboard.SetSimulationMode(false);
			}
		}
	}
}
