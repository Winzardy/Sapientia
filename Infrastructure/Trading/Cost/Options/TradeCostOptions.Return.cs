using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public sealed partial class TradeCostOptions
	{
		public override bool CanReturn(Tradeboard board, out TradeCostReturnError? error) =>
			options[selectedIndex].CanReturn(board, out error);

		internal override Task<bool> ReturnAsync(Tradeboard board, CancellationToken cancellationToken = default) =>
			options[selectedIndex].ReturnAsync(board, cancellationToken);
	}
}
