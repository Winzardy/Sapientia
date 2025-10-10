using Sapientia.Pooling;

namespace Trading
{
	public static partial class TradeUtility
	{
		internal static bool CanExecute(this TradeConfig trade, Tradeboard tradeboard, out TradeExecuteError? error)
		{
			var result = true;
			error = null;

			if (!trade.cost.CanExecute(tradeboard, out var payError))
				result = false;

			if (!trade.reward.CanExecute(tradeboard, out var receiveError))
				result = false;

			if (!result)
				error = new TradeExecuteError(payError, receiveError);

			return result;
		}

		internal static bool Execute(this TradeConfig trade, Tradeboard tradeboard)
		{
			// Сначала платим
			var success = trade.cost.Execute(tradeboard);
			if (!success)
				return false;

			// Потом получаем
			success = trade.reward.Execute(tradeboard);

			// Если что-то пошло не по плану возвращаем
			if (!success)
				trade.cost.ExecuteRefund(tradeboard);

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
		public static TradeExecuteError? NotError => null;

		public override string ToString() => ToString(", ");

		public string ToString(string separator)
		{
			using (StringBuilderPool.Get(out var sb))
			{
				if (payError.HasValue)
				{
					sb.Append(payError);

					if (receiveError.HasValue)
						sb.Append(separator);
				}

				if (receiveError.HasValue)
				{
					sb.Append(payError);
				}

				return sb.ToString();
			}
		}
	}
}
