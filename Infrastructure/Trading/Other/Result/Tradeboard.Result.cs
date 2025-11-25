using System.Collections.Generic;
using Sapientia.Collections;
using Sapientia.Pooling;
using Trading.Result;

namespace Trading
{
	public sealed partial class Tradeboard
	{
		private TradeRawResult _rawResult;

		private List<ITradeResultHandleToken> _resultHandleTokens;

		public ref readonly TradeRawResult RawResult => ref _rawResult;

		public THandle RegisterCostHandle<TCost, THandle>(TCost source)
			where TCost : TradeCost
			where THandle : class, ITradeCostResultHandle<TCost>, new()
		{
			var handle = Pool<THandle>.Get();
			handle.Bind(source);

			// Токен чтобы потом отпустить в пул
			_resultHandleTokens ??= ListPool<ITradeResultHandleToken>.Get();
			var token = Pool<TradeCostResultHandleToken<THandle>>.Get();
			token.Bind(handle);
			_resultHandleTokens.Add(token);

			_rawResult.costs ??= ListPool<ITradeCostResultHandle>.Get();
			_rawResult.costs.Add(handle);

			return handle;
		}

		public THandle RegisterRewardHandle<TReward, THandle>(TReward source)
			where TReward : TradeReward
			where THandle : class, ITradeRewardResultHandle<TReward>, new()
		{
			var handle = Pool<THandle>.Get();
			handle.Bind(source);

			// Токен чтобы потом отпустить в пул
			_resultHandleTokens ??= ListPool<ITradeResultHandleToken>.Get();
			var token = Pool<TradeRewardResultHandleToken<THandle>>.Get();
			token.Bind(handle);
			_resultHandleTokens.Add(token);

			_rawResult.rewards ??= ListPool<ITradeRewardResultHandle>.Get();
			_rawResult.rewards.Add(handle);

			return handle;
		}

		/// <summary>
		/// Перенос обработку (handle) результата на другую сделку (tradeboard)
		/// </summary>
		/// <returns>Как Tradeboard отпустят, TradeRawResult становится не актуальным!</returns>
		public TradeRawResult TransferResultHandlesTo(Tradeboard newBoard)
		{
			newBoard.ApplyTransfer(_resultHandleTokens);
			StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _resultHandleTokens);
			return _rawResult;
		}

		private void ApplyTransfer(List<ITradeResultHandleToken> handles)
		{
			if (handles.IsNullOrEmpty())
				return;

			_resultHandleTokens ??= ListPool<ITradeResultHandleToken>.Get();
			_resultHandleTokens.AddRange(handles);
		}

		public TradeResultSnapshot SnapshotResult() => new(Id, in _rawResult);

		private void OnReleaseResultHandle()
		{
			if (_resultHandleTokens != null)
			{
				foreach (var token in _resultHandleTokens)
					token.ReturnToPool();

				StaticObjectPoolUtility.ReleaseAndSetNull(ref _resultHandleTokens);
			}

			_rawResult.costs?.ReleaseToStaticPool();
			_rawResult.rewards?.ReleaseToStaticPool();
			_rawResult = default;
		}
	}

	/// <summary>
	/// Результат который формируется, чтобы получить слепок, нужно вызвать <see cref="Tradeboard.SnapshotResult()"/>
	/// </summary>
	public struct TradeRawResult
	{
		public List<ITradeCostResultHandle> costs;
		public List<ITradeRewardResultHandle> rewards;
	}
}
