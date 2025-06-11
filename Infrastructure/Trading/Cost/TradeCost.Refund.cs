namespace Trading
{
	public abstract partial class TradeCost
	{
		internal bool CanExecuteRefund(Tradeboard board, out TradeCostRefundError? error)
			=> CanRefund(board, out error);

		/// <inheritdoc cref="RefundAsync"/>
		internal bool ExecuteRefund(Tradeboard board)
			=> Refund(board);

		protected virtual bool CanRefund(Tradeboard board, out TradeCostRefundError? error)
		{
			error = null;
			return false;
		}

		/// <summary>
		/// Возвращаем, что заплатили
		/// </summary>
		/// <returns>Получилось ли?</returns>
		protected virtual bool Refund(Tradeboard board) => true;
	}

	public readonly struct TradeCostRefundError
	{
		public readonly string category;
		public readonly int code;
		public readonly object rawData;

		public TradeCostRefundError(string category, int code, object rawData = null)
		{
			this.category = category;
			this.code = code;
			this.rawData = rawData;
		}

		public TradeCostRefundError(string category, object rawData = null) : this(category, 0, rawData)
		{
		}
	}
}
