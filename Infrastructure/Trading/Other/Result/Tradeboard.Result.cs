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
		/// <param name="onlyHandle">Нужно ли результат переносить? если <c>false</c> то переносим</param>
		/// <returns>Как Tradeboard отпустят, TradeRawResult становится не актуальным!</returns>
		public TradeRawResult TransferResultHandlesTo(Tradeboard newBoard, bool onlyHandle = true)
		{
			newBoard.ApplyTransfer(_resultHandleTokens, in _rawResult, onlyHandle);
			StaticObjectPoolUtility.ReleaseAndSetNullSafe(ref _resultHandleTokens);
			return _rawResult;
		}

		private void ApplyTransfer(List<ITradeResultHandleToken> handles, in TradeRawResult rawResult, bool onlyHandle)
		{
			if (!handles.IsNullOrEmpty())
			{
				_resultHandleTokens ??= ListPool<ITradeResultHandleToken>.Get();
				_resultHandleTokens.AddRange(handles);
			}

			if (onlyHandle)
				return;

			if (!rawResult.costs.IsNullOrEmpty())
			{
				_rawResult.costs ??= ListPool<ITradeCostResultHandle>.Get();
				_rawResult.costs.AddRange(rawResult.costs);
			}

			if (!rawResult.rewards.IsNullOrEmpty())
			{
				_rawResult.rewards ??= ListPool<ITradeRewardResultHandle>.Get();
				_rawResult.rewards.AddRange(rawResult.rewards);
			}
		}

		public TradeResultSnapshot SnapshotResult() => new(Id, in _rawResult);

		internal void RefundResult(bool clear = true)
		{
			var snapshot = SnapshotResult();

			foreach (var reward in snapshot.rewards)
				reward.Return(this);
			foreach (var cost in snapshot.costs)
				cost.Refund(this);

			if (clear)
				ClearResult();
		}

		internal void ClearResult() => _rawResult = default;

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
