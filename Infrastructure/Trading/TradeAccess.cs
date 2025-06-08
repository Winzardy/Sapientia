using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	/// <summary>
	/// Защита от дурака: только через этот класс можно вызывать ExecuteAsync
	/// </summary>
	public static class TradeAccess
	{
		public static Task<bool> ExecuteAsync(TradeCost cost, Tradeboard board, CancellationToken cancellationToken)
			=> cost.ExecuteAsync(board, cancellationToken);

		public static Task<bool> ExecuteAsync(TradeReward reward, Tradeboard board, CancellationToken cancellationToken)
			=> reward.ExecuteAsync(board, cancellationToken);

		public static Task<bool> ExecuteAsync(TradeEntry trade, Tradeboard board, CancellationToken cancellationToken)
			=> TradeUtility.ExecuteAsync(trade, board, cancellationToken);
	}
}
