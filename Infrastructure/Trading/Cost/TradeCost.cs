using System;

namespace Trading
{
	[Serializable]
	public abstract partial class TradeCost
	{
		/// <summary>
		/// Участвует в сортировке в <see cref="TradeCostCollection"/>
		/// </summary>
		public virtual int Priority => TradeCostPriority.NORMAL;

		internal bool CanExecute(Tradeboard board, out TradePayError? error)
		{
			OnBeforePayCheck(board);
			var result = CanPay(board, out error);
			OnAfterPayCheck(board);
			return result;
		}

		internal bool Execute(Tradeboard board)
		{
			OnBeforePay(board);
			var result = Pay(board);
			OnAfterPay(board);
			return result;
		}

		/// <summary>
		/// Доступно ли для продажи? пример: есть ли у игрока 100 монет?
		/// </summary>
		protected abstract bool CanPay(Tradeboard board, out TradePayError? error);

		/// <summary>
		/// Плати!
		/// </summary>
		/// <returns>Успешность</returns>
		protected abstract bool Pay(Tradeboard board);

		protected virtual void OnBeforePayCheck(Tradeboard board)
		{
		}

		protected virtual void OnAfterPayCheck(Tradeboard board)
		{
		}

		protected virtual void OnBeforePay(Tradeboard board)
		{
		}

		protected virtual void OnAfterPay(Tradeboard board)
		{
		}
	}

	public readonly struct TradePayError
	{
		public const string CANCELLED_CATEGORY = "Cancelled";
		public const string NOT_IMPLEMENTED_CATEGORY = "NotImplemented";
		public const int CANCELLED_CODE = -1;

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

		public static TradePayError NotImplemented = new(NOT_IMPLEMENTED_CATEGORY);
		public static TradePayError Cancelled = new(CANCELLED_CATEGORY, CANCELLED_CODE);
		public static TradePayError? NotError => null;

		public bool IsCancelled() => category == CANCELLED_CATEGORY && code == CANCELLED_CODE;

		public override string ToString() => $"TradePayError: category: {category}, code: {code}, rawData: {rawData}";
	}
}
