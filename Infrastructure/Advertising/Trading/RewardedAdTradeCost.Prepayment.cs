using System.Threading;
using System.Threading.Tasks;
using Advertising;
using Content;

namespace Trading.Advertising
{
	public partial class RewardedAdTradeCost : IPrepayment
	{
		bool IPrepayment.CanPrepay(Tradeboard board, out TradePayError? error)
		{
			error = null;

			var success = AdManager.CanShow(placement, out var localError);

			if (localError.HasValue)
				error = new TradePayError(ERROR_CATEGORY, (int) localError.Value.code, localError);

			return success;
		}

		async Task<ITradeReceipt> IPrepayment.PrepayAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			//TODO: может быть какую-то защиту от дурака, что нельзя посмотреть рекламу за секунду)
			var success = await AdManager.ShowAsync(placement, cancellationToken);
			if (success)
			{
				return new RewardedAdTradeReceipt
				{
					placementId = placement.Read().Id
				};
			}

			return null;
		}
	}

	public class RewardedAdTradeReceipt : ITradeReceipt
	{
		public string placementId;
		public override string ToString() => $"AD Receipt: placement: {placementId}";
		public string Id => placementId;
	}
}
