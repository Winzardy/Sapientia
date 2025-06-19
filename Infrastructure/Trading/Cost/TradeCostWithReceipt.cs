using System.Threading;
using System.Threading.Tasks;
using Sapientia.Extensions;

namespace Trading
{
	public abstract class TradeCostWithReceipt<T> : TradeCost, ITradeCostWithReceipt
		where T : struct, ITradeReceipt
	{
		protected abstract string ReceiptId { get; }

		protected virtual int ReceiptCount => 1;

		protected sealed override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			return TradeReceiptRegistry<T>.CanIssue(board, ReceiptId, out error);
		}

		protected sealed override bool Pay(Tradeboard board)
		{
			return TradeReceiptRegistry<T>.Issue(board, ReceiptId);
		}

		protected abstract bool CanFetch(Tradeboard board, out TradePayError? error);

		protected abstract Task<T?> FetchAsync(Tradeboard board, CancellationToken cancellationToken);

		bool ITradeCostWithReceipt.CanFetch(Tradeboard board, out TradePayError? error) => CanFetch(board, out error);

		async Task<ITradeReceipt> ITradeCostWithReceipt.FetchAsync(Tradeboard board, CancellationToken cancellationToken)
			=> await FetchAsync(board, cancellationToken);
	}

	public interface ITradeCostWithReceipt
	{
		public bool CanFetch(Tradeboard board, out TradePayError? error);
		public Task<ITradeReceipt> FetchAsync(Tradeboard board, CancellationToken cancellationToken);
	}

	public interface ITradeReceipt
	{
		public string Key { get; }

		// Намеренный хак чтобы избежать каста
		public void Register(ITradingModel model, string tradeId);
	}

	public static class TradeReceiptUtility
	{
		public static void Register(this ITradeReceipt[] receipts, ITradingModel model, string tradeId)
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			for (int i = 0; i < receipts.Length; i++)
				receipts[i].Register(model, tradeId);
		}

		public static void Register<T>(this T receipt, ITradingModel model, string tradeId)
			where T : struct, ITradeReceipt
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			TradeReceiptRegistry<T>.Register(model, tradeId, in receipt);
		}
	}
}
