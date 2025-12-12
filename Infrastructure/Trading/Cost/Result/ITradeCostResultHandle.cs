using Sapientia.Pooling;

namespace Trading.Result
{
	/// <summary>
	/// Представляет временную прослойку между <see cref="TradeCost"/> и итоговым результатом.
	/// Позволяет при необходимости получить запечённый результат через <see cref="Bake"/>
	/// </summary>
	public interface ITradeCostResultHandle : IPoolable
	{
		public TradeCost Source { get; }
		public ITradeCostResult Bake();
	}

	public interface ITradeCostResultHandle<in TCost> : ITradeCostResultHandle
		where TCost : TradeCost
	{
		internal void Bind(TCost source);
	}
}
