using System.Threading;
using System.Threading.Tasks;
using Sapientia.Extensions;

namespace Trading
{
	public abstract class TradeCostWithReceipt<T> : TradeCost, ITradeCostWithReceipt
		where T : struct, ITradeReceipt
	{
		protected abstract string ReceiptId { get; }

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
		public void Registry(ITradingModel model, string tradeId);
	}

	public static class TradeReceiptUtility
	{
		public static void Registry(this ITradeReceipt[] receipts, ITradingModel model, string tradeId)
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			for (int i = 0; i < receipts.Length; i++)
				receipts[i].Registry(model, tradeId);
		}

		public static void Registry<T>(this T receipt, ITradingModel model, string tradeId)
			where T : struct, ITradeReceipt
		{
			if (tradeId.IsNullOrEmpty())
				throw TradingDebug.NullException("Trade ID cannot be null or empty");

			TradeReceiptRegistry<T>.Registry(model, tradeId, in receipt);
		}
	}

	public interface ITradingModel
	{
		public ITradeReceiptRegistry<T> Get<T>() where T : struct, ITradeReceipt;
	}

	public interface ITradeRegistry
	{
	}

	public interface ITradeReceiptRegistry<T> : ITradeRegistry
		where T : struct, ITradeReceipt
	{
		public void Register(string tradeId, in T receipt);
		public bool CanIssue(Tradeboard board, string key); // TODO: Добавить out error!
		public bool Issue(Tradeboard board, string key);
	}

	public static class TradeReceiptRegistry<T>
		where T : struct, ITradeReceipt
	{
		public static void Registry(ITradingModel model, string tradeId, in T receipt)
		{
			var registry = model.Get<T>();
			registry?.Register(tradeId, in receipt);
		}

		public static bool CanIssue(Tradeboard board, string key, out TradePayError? error)
		{
			error = null;
			var model = board.Get<ITradingModel>();
			var registry = model.Get<T>();

			if (registry != null)
				return registry.CanIssue(board, key);

			TradingDebug.LogWarning($"Not found receipt registry by type [ {typeof(T)} ]");
			return true;
		}

		public static bool Issue(Tradeboard board, string key)
		{
			var model = board.Get<ITradingModel>();
			var registry = model.Get<T>();

			if (registry != null)
				return registry.Issue(board, key);

			TradingDebug.LogWarning($"Not found receipt registry by type [ {typeof(T)} ]");
			return true;
		}
	}
}
