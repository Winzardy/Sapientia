using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trading.Management
{
	public sealed class TradeManagement
	{
		internal bool CanExecute(TradeEntry trade, out TradeExecuteError? error)
		{
			var result = true;
			error = null;

			if (!trade.cost.CanPay(out var payError))
				result = false;

			if (!trade.reward.CanReceive(out var receiveError))
				result = false;

			if (!result)
				error = new TradeExecuteError(payError, receiveError);

			return result;
		}

		internal async Task<bool> ExecuteAsync(TradeEntry trade, CancellationToken cancellationToken)
		{
			var success = await trade.cost.ExecuteAsync(cancellationToken);
			if (!success)
				return false;

			success = await trade.reward.ReceiveAsync(cancellationToken);
			return success;
		}

		internal void Register(TradeEntry trade)
		{
			throw new NotImplementedException();
		}

		internal void Unregister(TradeEntry trade)
		{
			throw new NotImplementedException();
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
