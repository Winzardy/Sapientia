using Sapientia;

namespace Trading
{
	public interface ITradingNode : IVirtualTimeProvider
	{
		ITradeReceiptRegistry<T> GetRegistry<T>() where T : struct, ITradeReceipt;

		int GetCurrentProgress(string key, in TradeProgressionScheme scheme);
		void IncrementProgress(string key, in TradeProgressionScheme scheme);
		void ResetProgress(string key, in TradeProgressionScheme scheme);
	}

	public interface ITradeRegistry
	{
	}

	public interface ITradeReceiptRegistry<T> : ITradeRegistry
		where T : struct, ITradeReceipt
	{
		void Register(string tradeId, in T receipt);
		bool CanIssue(Tradeboard board, string key); // TODO: Добавить out error!
		bool Issue(Tradeboard board, string key);
	}
}
