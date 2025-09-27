using JetBrains.Annotations;

namespace Trading
{
	/// <summary>
	/// Защита от дурака: только через этот класс можно вызывать Execute
	/// </summary>
	public static class TradeAccess
	{
		public static bool CanPay([CanBeNull] TradeCost cost, Tradeboard board, out TradePayError? error)
		{
			if (cost == null)
			{
				error = null;
				return true;
			}

			return cost.CanExecute(board, out error);
		}

		public static bool Pay([CanBeNull] TradeCost cost, Tradeboard board)
		{
			return cost == null || cost.Execute(board);
		}

		public static bool CanReceive(TradeReward reward, Tradeboard board, out TradeReceiveError? error)
			=> reward.CanExecute(board, out error);

		public static bool Receive(TradeReward reward, Tradeboard board)
			=> reward.Execute(board);

		public static bool CanExecute(in TradeConfig config, Tradeboard board, out TradeExecuteError? error)
			=> config.CanExecute(board, out error);

		public static bool Execute(in TradeConfig config, Tradeboard board)
			=> config.Execute(board);
	}
}
