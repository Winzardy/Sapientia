using Content;
using Sapientia;

namespace Trading
{
	/// <summary>
	/// Защита от дурака: только через этот класс можно вызывать Execute
	/// </summary>
	public class TradeManager
#if CLIENT
		: StaticWrapper<ITradeGateway>
#endif
	{
		public static bool CanPay(TradeCost cost, Tradeboard board, out TradePayError? error)
		{
			if (!board.IsSimulationMode)
				throw TradingDebug.Exception($"CanPay check requires simulation mode (board [ {board.Id} ])");

#if CLIENT
			if (board.IsFetchMode)
			{
				if (cost is IInterceptableTradeCost interceptable)
				{
					if (interceptable.ShouldIntercept(board))
					{
						error = null;
						return true;
					}
				}
			}
#endif

			return cost.CanExecute(board, out error);
		}

		public static bool Pay(TradeCost cost, Tradeboard board)
		{
			using (board.TradeModeScope())
				return cost.Execute(board);
		}

		public static bool CanReceive(TradeReward reward, Tradeboard board, out TradeReceiveError? error)
			=> reward.CanExecute(board, out error);

		public static bool Receive(TradeReward reward, Tradeboard board)
		{
			using (board.TradeModeScope())
				return reward.Execute(board);
		}

		public static bool CanExecute(in TradeConfig config, Tradeboard board, out TradeExecuteError? error)
			=> config.CanExecute(board, out error);

		public static bool Execute(in TradeConfig config, Tradeboard board)
		{
			using (board.TradeModeScope())
				return config.Execute(board);
		}

#if CLIENT
		public static bool CanFetch(in ContentReference<TradeCost> costRef, Tradeboard tradeboard, out TradePayError? error)
			=> CanFetch(costRef, costRef.GetTradeId(), tradeboard, out error);

		public static bool CanFetch(TradeCost cost, string tradeId, Tradeboard tradeboard, out TradePayError? error)
			=> _instance!.CanFetch(cost, tradeId, tradeboard, out error);

		public static bool CanFetch(TradeConfig trade, Tradeboard tradeboard, out TradeExecuteError? error)
			=> _instance!.CanFetch(trade, tradeboard, out error);

		public static void PushReceipts(Tradeboard tradeboard)
			=> _instance!.PushReceipts(tradeboard);
#endif
	}
}
