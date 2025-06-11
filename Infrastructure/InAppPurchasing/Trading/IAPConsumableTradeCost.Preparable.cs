using System.Threading;
using System.Threading.Tasks;
using InAppPurchasing;

namespace Trading.InAppPurchasing
{
	public partial class IAPConsumableTradeCost : ITradePreparable
	{
		bool ITradePreparable.CanPrepare(Tradeboard board, out TradePayError? error)
		{
			error = null;
			var success = IAPManager.CanPurchase(product, out var localError);

			if (localError != null)
				error = new TradePayError(ERROR_CATEGORY, (int) localError.Value.code, localError);

			return success;
		}

		async Task<ITradeReceipt> ITradePreparable.PrepareAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			var result = await IAPManager.PurchaseAsync(product, cancellationToken);
			if (result.success)
				return new IAPTradeReceipt(result.receipt);
			return null;
		}
	}
}
