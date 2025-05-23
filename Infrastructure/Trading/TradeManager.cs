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
		public static bool CanExecute(TradeEntry entry, out TradeExecuteError? error) => management.CanExecute(entry, out error);

		public static Task<bool> ExecuteAsync(TradeEntry entry, CancellationToken cancellationToken = default) =>
			management.ExecuteAsync(entry, cancellationToken);
	}
}
