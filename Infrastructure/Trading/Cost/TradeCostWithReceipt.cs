using System.Threading;
using System.Threading.Tasks;
using Sapientia.Collections;
using Sapientia.Extensions;

namespace Trading
{
	public abstract class TradeCostWithReceipt<T> : TradeCost, ITradeCostWithReceipt
		where T : struct, ITradeReceipt
	{
		private const string ERROR_CATEGORY = "Can't issue receipt";

		protected sealed override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			if (board.IsFetching)
				return CanFetch(board, out error);

			if (!board.Contains<ITradingNode>())
			{
				TradingDebug.LogError("Not found trading service...");

				error = new TradePayError(ERROR_CATEGORY, 0, null);
				return false;
			}

			var backend = board.Get<ITradingNode>();
			var registry = backend.GetRegistry<T>();
			var canIssue = registry.CanIssue(board, GetReceiptKey(board.Id));
			error = canIssue ? null : new TradePayError(ERROR_CATEGORY, 0, null);
			return canIssue;
		}

		protected sealed override bool Pay(Tradeboard board)
		{
			if (!board.Contains<ITradingNode>())
			{
				TradingDebug.LogError("Not found trading service...");
				return false;
			}

			var backend = board.Get<ITradingNode>();
			var registry = backend.GetRegistry<T>();
			return registry.Issue(board, GetReceiptKey(board.Id));
		}

		protected abstract string GetReceiptKey(string tradeId);

		protected abstract bool CanFetch(Tradeboard board, out TradePayError? error);

		protected abstract Task<T?> FetchAsync(Tradeboard board, CancellationToken cancellationToken);

		bool ITradeCostWithReceipt.CanFetch(Tradeboard board, out TradePayError? error)
		{
			OnBeforeFetchCheck(board);
			var result = CanFetch(board, out error);
			OnAfterFetchCheck(board);
			return result;
		}

		async Task<ITradeReceipt> ITradeCostWithReceipt.FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			OnBeforeFetch(board);
			var result = await FetchAsync(board, cancellationToken);
			OnAfterFetch(board);
			return result;
		}

		protected virtual void OnBeforeFetchCheck(Tradeboard board)
		{
		}

		protected virtual void OnAfterFetchCheck(Tradeboard board)
		{
		}

		protected virtual void OnBeforeFetch(Tradeboard board)
		{
		}

		protected virtual void OnAfterFetch(Tradeboard board)
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
		public string GetKey(string tradeId);

		public bool NeedPush() => true;
	}

	public static class TradeReceiptUtility
	{
		public static void Register<T>(this ITradingNode node, T[] receipts, string tradeId)
			where T : struct, ITradeReceipt
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			if (receipts.IsNullOrEmpty())
				return;

			for (int i = 0; i < receipts.Length; i++)
				node.Register(receipts[i], tradeId);
		}

		public static void Register<T>(this ITradingNode node, in T receipt, string tradeId)
			where T : struct, ITradeReceipt
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			var registry = node.GetRegistry<T>();

			if (registry == null)
				throw TradingDebug.Exception($"Not found receipt registry by type [ {typeof(T)} ]");

			registry.Register(tradeId, in receipt);
		}
	}
}
