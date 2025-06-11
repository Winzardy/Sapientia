using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public sealed partial class TradeCostOptions
	{
		protected override bool CanRefund(Tradeboard board, out TradeCostRefundError? error) =>
			options[selectedIndex].CanExecuteRefund(board, out error);

		protected override Task<bool> RefundAsync(Tradeboard board, CancellationToken cancellationToken = default) =>
			options[selectedIndex].ExecuteRefundAsync(board, cancellationToken);
	}
}
