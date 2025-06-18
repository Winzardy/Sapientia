namespace Trading
{
	public static partial class TradeUtility
	{
		internal static bool CanExecute(this TradeEntry trade, Tradeboard board, out TradeExecuteError? error)
		{
			var result = true;
			error = null;

			if (!trade.cost.CanExecute(board, out var payError))
				result = false;

			if (!trade.reward.CanExecute(board, out var receiveError))
				result = false;

			if (!result)
				error = new TradeExecuteError(payError, receiveError);

			return result;
		}

		internal static bool Execute(this TradeEntry trade, Tradeboard board)
		{
			// Сначала платим
			var success = trade.cost.Execute(board);
			if (!success)
				return false;

			// Потом получаем
			success = trade.reward.Execute(board);

			// Если что-то пошло не по плану возвращаем
			if (!success)
				trade.cost.ExecuteRefund(board);

			return success;
		}
	}

	public struct TradeExecuteError
	{
		public TradePayError? payError;
		public TradeReceiveError? receiveError;

		public TradeExecuteError(TradePayError? payError, TradeReceiveError? receiveError)
		{
			this.payError = payError;
			this.receiveError = receiveError;
		}

		public static TradeExecuteError NotImplemented = new(TradePayError.NotImplemented, TradeReceiveError.NotImplemented);
	}
}
