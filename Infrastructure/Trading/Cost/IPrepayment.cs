using System.Threading;
using System.Threading.Tasks;
using Sapientia;

namespace Trading
{
	public interface IPrepayment
	{
		public bool CanPrepay(Tradeboard board, out TradePayError? error);

		public Task<ITradeReceipt> PrepayAsync(Tradeboard board, CancellationToken cancellationToken);
	}

	public interface ITradeReceipt : IIdentifiable
	{
	}

	public interface ITradeReceiptRegistry<in T>
		where T : ITradeReceipt
	{
		public bool Registry(T receipt);

		public bool CanIssue(Tradeboard board, string key);
		public bool Issue(Tradeboard board, string key);
	}

	public interface ITradeRegistry
	{
		public void Registry(ITradeReceipt receipt);
		bool CanIssue<T>(Tradeboard board, string key) where T : ITradeReceipt;
		bool Issue<T>(Tradeboard board,string key) where T : ITradeReceipt;
	}
}
