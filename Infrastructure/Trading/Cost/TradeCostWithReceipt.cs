using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Trading
{
	public abstract class TradeCostWithReceipt<T> : TradeCost, ITradeCostWithReceipt
		where T : struct, ITradeReceipt
	{
		protected abstract string ReceiptId { get; }

		protected sealed override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			var canIssue = false;

			OnBeforePayCheck(board);
			{
				error = null;
				if (!board.Contains<ITradingBackend>())
				{
					TradingDebug.LogError("Not found trading service...");
				}
				else
				{
					var backend = board.Get<ITradingBackend>();
					var registry = backend.Get<T>();
					canIssue = registry.CanIssue(board, ReceiptId);
				}
			}

			OnAfterPayCheck(board);

			return canIssue;
		}

		protected sealed override bool Pay(Tradeboard board)
		{
			var issue = false;
			OnBeforePay(board);
			{
				if (!board.Contains<ITradingBackend>())
				{
					TradingDebug.LogError("Not found trading service...");
				}
				else
				{
					var backend = board.Get<ITradingBackend>();
					var registry = backend.Get<T>();
					issue = registry.Issue(board, ReceiptId);
				}
			}

			OnAfterPay(board);
			return issue;
		}

		protected abstract bool CanFetch(Tradeboard board, out TradePayError? error);

		protected abstract Task<T?> FetchAsync(Tradeboard board, CancellationToken cancellationToken);

		bool ITradeCostWithReceipt.CanFetch(Tradeboard board, out TradePayError? error) => CanFetch(board, out error);

		async Task<ITradeReceipt> ITradeCostWithReceipt.FetchAsync(Tradeboard board, CancellationToken cancellationToken)
			=> await FetchAsync(board, cancellationToken);

		protected virtual void OnBeforePayCheck(Tradeboard board)
		{
		}

		protected virtual void OnBeforePay(Tradeboard board)
		{
		}

		protected virtual void OnAfterPayCheck(Tradeboard board)
		{
		}

		protected virtual void OnAfterPay(Tradeboard board)
		{
		}
	}

	public interface ITradeCostWithReceipt
	{
		public bool CanFetch(Tradeboard board, out TradePayError? error);
		public Task<ITradeReceipt> FetchAsync(Tradeboard board, CancellationToken cancellationToken);
	}

	public interface ITradeReceipt
	{
		public string Key { get; }

		public bool NeedPush() => true;
	}

	public static class TradeReceiptUtility
	{
		public static void Register<T>(this ITradingBackend backend, T[] receipts, string tradeId)
			where T : struct, ITradeReceipt
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			if (receipts.IsNullOrEmpty())
				return;

			for (int i = 0; i < receipts.Length; i++)
				backend.Register(receipts[i], tradeId);
		}

		public static void Register<T>(this ITradingBackend backend, in T receipt, string tradeId)
			where T : struct, ITradeReceipt
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			var registry = backend.Get<T>();

			if (registry == null)
				throw TradingDebug.Exception($"Not found receipt registry by type [ {typeof(T)} ]");

			registry.Register(tradeId, in receipt);
		}
	}
}
