using Sapientia.Pooling;
using Trading.Result;

namespace Trading
{
	internal class TradeRewardResultHandleToken<T> : ITradeResultHandleToken, IPoolable
		where T : class, ITradeRewardResultHandle, new()
	{
		private T _result;

		public void Bind(T result)
		{
			_result = result;
		}

		public void Release()
		{
			Pool<T>.Release(_result);
			_result = null;
		}

		public void ReturnToPool()
		{
			Pool<TradeRewardResultHandleToken<T>>.Release(this);
		}
	}

	internal class TradeCostResultHandleToken<T> : ITradeResultHandleToken, IPoolable
		where T : class, ITradeCostResultHandle, new()
	{
		private T _result;

		public void Bind(T result)
		{
			_result = result;
		}

		public void Release()
		{
			Pool<T>.Release(_result);
			_result = null;
		}

		public void ReturnToPool()
		{
			Pool<TradeCostResultHandleToken<T>>.Release(this);
		}
	}
}
