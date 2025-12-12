using Sapientia.Pooling;

namespace Trading.Result
{
	/// <summary>
	/// Представляет временную прослойку между <see cref="TradeReward"/> и итоговым результатом.
	/// Позволяет при необходимости получить запечённый результат через <see cref="Bake"/>
	/// </summary>
	public interface ITradeRewardResultHandle : IPoolable
	{
		public TradeReward Source { get; }
		public ITradeRewardResult Bake();
	}

	public interface ITradeRewardResultHandle<in TReward> : ITradeRewardResultHandle
		where TReward : TradeReward
	{
		internal void Bind(TReward source);
	}
}
