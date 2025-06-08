using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Sapientia;
using Trading.Management;

namespace Trading
{
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

		public static bool CanExecute(TradeEntry entry, Tradeboard board, out TradeExecuteError? error)
			=> management.CanExecute(entry, board, out error);

		public static Task<bool> ExecuteAsync(TradeEntry entry, Tradeboard board, CancellationToken cancellationToken = default) =>
			management.ExecuteAsync(entry, board, cancellationToken);
	}

	/// <summary>
	/// Защита от дурака: только через этот класс можно вызывать ExecuteAsync
	/// </summary>
	public static class TradeAccess
	{
		public static Task<bool> ExecuteAsync(TradeCost cost, Tradeboard board, CancellationToken cancellationToken)
			=> cost.ExecuteAsync(board, cancellationToken);

		public static Task<bool> ExecuteAsync(TradeReward reward, Tradeboard board, CancellationToken cancellationToken)
			=> reward.ExecuteAsync(board, cancellationToken);
	}
}
