using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	[Serializable]
	public abstract partial class TradeCost
	{
		/// <summary>
		/// Участвует в сортировке в <see cref="TradeCostCollection"/>
		/// </summary>
		public virtual int Priority => TradeCostPriority.NORMAL;

		public virtual bool Prepayment => false;
		public bool CanExecute(Tradeboard board, out TradePayError? error) => CanPay(board, out error);
		internal Task<bool> ExecuteAsync(Tradeboard board, CancellationToken cancellationToken) => PayAsync(board, cancellationToken);

		/// <summary>
		/// Доступно ли для продажи? пример: есть ли у игрока 100 монет?
		/// </summary>
		protected abstract bool CanPay(Tradeboard board, out TradePayError? error);

		/// <summary>
		/// Плати!
		/// </summary>
		/// <returns>Успешность</returns>
		protected abstract Task<bool> PayAsync(Tradeboard board, CancellationToken cancellationToken);
	}

	public readonly struct TradePayError
	{
		public readonly string category;
		public readonly int code;
		public readonly object rawData;

		public TradePayError(string category, int code, object rawData = null)
		{
			this.category = category;
			this.code = code;
			this.rawData = rawData;
		}

		public TradePayError(string category, object rawData = null) : this(category, 0, rawData)
		{
		}
	}
}
