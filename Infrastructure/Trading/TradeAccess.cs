namespace Trading
{
	/// <summary>
	/// Защита от дурака: только через этот класс можно вызывать Execute
	/// </summary>
	public static class TradeAccess
	{
		public static bool CanPay(TradeCost cost, Tradeboard board, out TradePayError? error)
			=> cost.CanExecute(board, out error);

		public static bool Pay(TradeCost cost, Tradeboard board)
			=> cost.Execute(board);

		public static bool CanReceive(TradeReward reward, Tradeboard board, out TradeReceiveError? error)
			=> reward.CanExecute(board, out error);

		public static bool Receive(TradeReward reward, Tradeboard board)
			=> reward.Execute(board);

		public static bool CanExecute(in TradeEntry entry, Tradeboard board, out TradeExecuteError? error)
			=> entry.CanExecute(board, out error);

		public static bool Execute(in TradeEntry entry, Tradeboard board)
			=> entry.Execute(board);
	}
}
