using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public abstract partial class TradeReward
	{
		/// <summary>
		/// Участвует в сортировке в <see cref="TradeRewardCollection"/>
		/// </summary>
		public virtual int Priority => TradeCostPriority.NORMAL;

		/// <summary>
		/// Доступно ли для получения? пример: нет места в инвентаре
		/// </summary>
		public abstract bool CanReceive(out TradeReceiveError? error);

		/// <summary>
		/// Выдать
		/// </summary>
		internal abstract Task<bool> ReceiveAsync(CancellationToken cancellationToken);
	}

	public readonly struct TradeReceiveError
	{
		public readonly string category;
		public readonly int code;
		public readonly object rawData;

		public TradeReceiveError(string category, int code, object rawData = null)
		{
			this.category = category;
			this.code = code;
			this.rawData = rawData;
		}

		public TradeReceiveError(string category, object rawData = null) : this(category, 0, rawData)
		{
		}
	}
}
