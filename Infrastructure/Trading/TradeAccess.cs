using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	/// <summary>
	/// Защита от дурака: только через этот класс можно вызывать ExecuteAsync
	/// </summary>
	public static class TradeAccess
	{
		public static bool Pay(TradeCost cost, Tradeboard board)
			=> cost.Execute(board);

		public static bool Pay(TradeReward reward, Tradeboard board)
			=> reward.Execute(board);
	}
}
