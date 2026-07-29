using System;
using System.Collections.Generic;
using Sapientia.Pooling;

namespace Trading
{
	public sealed partial class Tradeboard
	{
		private bool _tradeMode;

		/// <summary>
		/// Награды используемые в сделке в порядке очереди
		/// </summary>
		private Queue<TradeReward> _rewards;

		/// <summary>
		/// Цены используемые в сделке в порядке очереди
		/// </summary>
		private Queue<TradeCost> _costs;

		/// <summary>
		/// Режим сделки!
		/// </summary>
		public bool IsTradeMode { get => _tradeMode; }

		internal void RegisterInternal(TradeReward reward)
		{
			_rewards.Enqueue(reward);
		}

		internal void RegisterInternal(TradeCost cost)
		{
			_costs.Enqueue(cost);
		}

		private void OnBeginTrade()
		{
			if (_tradeMode)
				throw TradingDebug.Exception("Trade mode is already active");

			_rewards = QueuePool<TradeReward>.Get();
			_costs = QueuePool<TradeCost>.Get();

			_tradeMode = true;
		}

		private void OnFinishTrade()
		{
			if (!_tradeMode)
				throw TradingDebug.Exception("Trade mode is not active");

			foreach (var cost in _costs)
				if (cost is ITradeFinishHandler handler)
					handler.OnTradeFinished(this);

			foreach (var reward in _rewards)
				if (reward is ITradeFinishHandler handler)
					handler.OnTradeFinished(this);

			StaticObjectPoolUtility.ReleaseAndSetNull(ref _rewards);
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _costs);

			_tradeMode = false;
		}

		/// <inheritdoc cref="IsTradeMode"/>
		public TradeScope TradeModeScope() => new(this);

		public readonly struct TradeScope : IDisposable
		{
			private readonly Tradeboard _tradeboard;
			private readonly bool _value;

			public TradeScope(Tradeboard tradeboard, bool value = true)
			{
				_value = value;
				_tradeboard = tradeboard;

				if (!value)
					return;

				_tradeboard.OnBeginTrade();
			}

			public void Dispose()
			{
				if (!_value)
					return;

				_tradeboard.OnFinishTrade();
			}
		}
	}
}
