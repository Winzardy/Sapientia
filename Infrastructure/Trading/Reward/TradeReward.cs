using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	[Serializable]
	public abstract partial class TradeReward
	{
		/// <summary>
		/// Участвует в сортировке в <see cref="TradeRewardCollection"/>
		/// </summary>
		protected internal virtual int Priority => TradeCostPriority.NORMAL;

		internal bool CanExecute(Tradeboard board, out TradeReceiveError? error) => CanReceive(board, out error);
		internal bool Execute(Tradeboard board) => Receive(board);


		/// <summary>
		/// Доступно ли для получения? пример: нет места в инвентаре
		/// </summary>
		protected internal abstract bool CanReceive(Tradeboard board, out TradeReceiveError? error);

		/// <summary>
		/// Выдать
		/// </summary>
		protected abstract bool Receive(Tradeboard board);
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
