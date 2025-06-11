using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public static class TradeUtility
	{
		public static bool CanExecute(this in TradeEntry trade, Tradeboard board, out TradeExecuteError? error)
		{
			var result = true;
			error = null;

			if (!trade.cost.CanExecute(board, out var payError))
				result = false;

			if (!trade.reward.CanReceive(board, out var receiveError))
				result = false;

			if (!result)
				error = new TradeExecuteError(payError, receiveError);

			return result;
		}

		internal static async Task<bool> ExecuteAsync(this TradeEntry trade, Tradeboard board, CancellationToken cancellationToken)
		{
			// Сначала платим
			var success = await trade.cost.ExecuteAsync(board, cancellationToken);
			if (!success)
				return false;

			// Потом получаем
			success = await trade.reward.ExecuteAsync(board, cancellationToken);

			// Если что-то пошло не по плану возвращаем
			if (!success)
				await trade.cost.ExecuteRefundAsync(board, cancellationToken);

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
	}
}
