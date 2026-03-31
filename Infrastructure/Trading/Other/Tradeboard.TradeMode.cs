using System;
using System.Collections.Generic;
using Sapientia;
using Sapientia.Pooling;

namespace Trading
{
	public sealed partial class Tradeboard
	{
		private bool _tradeMode;

		private HashSet<TradeReward> _rewards;
		private HashSet<TradeCost> _costs;

		/// <summary>
		/// Режим сделки!
		/// </summary>
		public bool IsTradeMode { get => _tradeMode; }

		internal void Register(TradeReward reward)
		{
			if (!_rewards.Add(reward))
				throw TradingDebug.Exception("Already registered...");
		}

		internal void Register(TradeCost cost)
		{
			if (!_costs.Add(cost))
				throw TradingDebug.Exception("Already registered...");
		}

		private void OnBeginTrade()
		{
			if (_tradeMode)
				throw TradingDebug.Exception("Trade mode is already active");

			_rewards = HashSetPool<TradeReward>.Get();
			_costs   = HashSetPool<TradeCost>.Get();

			_tradeMode = true;
		}

		private void OnFinishTrade()
		{
			if (!_tradeMode)
				throw TradingDebug.Exception("Trade mode is not active");

			foreach (ITradeFinishHandler cost in _costs)
				cost.OnTradeFinished(this);
			foreach (ITradeFinishHandler reward in _rewards)
				reward.OnTradeFinished(this);

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
				_value      = value;
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
