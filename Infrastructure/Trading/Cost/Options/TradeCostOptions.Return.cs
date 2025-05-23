using System.Threading;
using System.Threading.Tasks;

namespace Trading
{
	public sealed partial class TradeCostOptions
	{
		public override bool CanReturn(out TradeCostReturnError? error) => options[selectedIndex].CanReturn(out error);

		internal override Task<bool> ReturnAsync(CancellationToken cancellationToken = default) =>
			options[selectedIndex].ReturnAsync(cancellationToken);
	}
}
