using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Sapientia;

namespace Trading
{
	using Management;

	//Подумать насчет контрактов, кейсы когда у нас подписка
	public sealed class TradeManager : StaticProvider<TradeManagement>
	{
		private static TradeManagement management
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance;
		}

		public static bool IsInitialized
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _instance != null;
		}

		public static void Register(TradeEntry entry) => management.Register(entry);
		public static void Unregister(TradeEntry entry) => management.Unregister(entry);

		public static bool CanExecute(TradeEntry entry, Tradeboard board, out TradeExecuteError? error)
			=> management.CanExecute(entry, board, out error);

		public static Task<bool> ExecuteAsync(TradeEntry entry, Tradeboard board, CancellationToken cancellationToken = default) =>
			management.ExecuteAsync(entry, board, cancellationToken);
	}

	public static class TradeManagerUtility
	{
		public static bool CanPay(this TradeCost cost, Tradeboard board, out TradePayError? error)
		{
			return cost.CanExecute(board, out error);
		}

		//TODO: трабла что ExecuteAsync ндао подумать
		public static bool Pay(this TradeCost cost, Tradeboard board)
		{
			return cost.ExecuteAsync(board, CancellationToken.None).Result;
		}
	}
}
