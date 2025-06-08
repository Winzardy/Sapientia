using System.Threading;
using System.Threading.Tasks;

namespace Trading.Management
{
	public sealed class TradeManagement
	{
		internal bool CanExecute(TradeEntry trade, Tradeboard board, out TradeExecuteError? error)
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

		internal async Task<bool> ExecuteAsync(TradeEntry trade, Tradeboard board, CancellationToken cancellationToken)
		{
			// Сначала платим
			var success = await trade.cost.ExecuteAsync(board, cancellationToken);
			if (!success)
				return false;

			// Потом получаем
			success = await trade.reward.ExecuteAsync(board, cancellationToken);

			// Если что-то пошло не по плану возвращаем
			if (!success)
				await trade.cost.ReturnAsync(board, cancellationToken);

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
