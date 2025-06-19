namespace Trading
{
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
		public static void Register(ITradingModel model, string tradeId, in T receipt)
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
			return true; // TODO: пока пропускаем такие кейсы из-за рекламы
		}

		public static bool Issue(Tradeboard board, string key)
		{
			var model = board.Get<ITradingModel>();
			var registry = model.Get<T>();

			if (registry != null)
				return registry.Issue(board, key);

			TradingDebug.LogWarning($"Not found receipt registry by type [ {typeof(T)} ]");
			return true; // TODO: пока пропускаем такие кейсы из-за рекламы
		}
	}
}
