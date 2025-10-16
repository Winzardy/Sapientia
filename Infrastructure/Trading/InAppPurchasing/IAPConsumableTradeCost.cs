using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Content;
using InAppPurchasing;
using Trading.Result;

namespace Trading.InAppPurchasing
{
	[Serializable]
	[TradeAccess(TradeAccessType.High)]
	public partial class IAPConsumableTradeCost : TradeCostWithReceipt<IAPTradeReceipt>, IInAppPurchasingTradeCost
	{
		private const string ERROR_CATEGORY = "InAppPurchasing";

		public ContentReference<IAPConsumableProductEntry> product;

		public override int Priority => TradeCostPriority.HIGH;

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;

			if (board.Contains<PurchaseReceipt>())
				return true;

			var success = IAPManager.CanPurchase(product, out var iapError);

			if (iapError != null)
				error = new TradePayError(ERROR_CATEGORY, (int) iapError.Value.code, iapError);

			return success;
		}

		protected override async Task<IAPTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			PurchaseReceipt? receipt = board.Contains<PurchaseReceipt>()
				? board.Get<PurchaseReceipt>()
				: null;

			if (!receipt.HasValue)
			{
				var result = await IAPManager.PurchaseAsync(product, cancellationToken);

				if (result.errorCode == PurchaseErrorCode.Canceled)
					throw new OperationCanceledException(cancellationToken);

				if (!result.success)
					return null;

				receipt = result.receipt;
			}

			return new IAPTradeReceipt(receipt.Value);
		}

		protected override void OnIssue(Tradeboard board, string receiptKey)
		{
			this.RegisterResultHandleTo(board, out IAPConsumableTradeCostResultHandle _);
		}

		public IAPProductEntry GetProductEntry() => product.Read();

		protected override string GetReceiptKey(string _) => product.ToReceiptKey();

		#region Restore

		protected override void OnBeforeFetchCheck(Tradeboard board) => TryEnableRestoreState(board);

		protected override void OnAfterFetchCheck(Tradeboard board) => TryDisableRestoreState(board);

		protected override void OnBeforeFetch(Tradeboard board) => TryEnableRestoreState(board);
		protected override void OnAfterFetch(Tradeboard board) => TryDisableRestoreState(board);

		protected override void OnBeforePayCheck(Tradeboard board) => TryEnableRestoreState(board);

		protected override void OnAfterPayCheck(Tradeboard board) => TryDisableRestoreState(board);

		protected override void OnBeforePay(Tradeboard board) => TryEnableRestoreState(board);
		protected override void OnAfterPay(Tradeboard board) => TryDisableRestoreState(board);

		private void TryEnableRestoreState(Tradeboard board)
		{
			if (!board.Contains<PurchaseReceipt>())
				return;

			var receipt = board.Get<PurchaseReceipt>();

			if (!receipt.isRestored)
				return;

			board.AddRestoreSource(receipt.transactionId);
		}

		private void TryDisableRestoreState(Tradeboard board)
		{
			if (!board.Contains<PurchaseReceipt>())
				return;

			var receipt = board.Get<PurchaseReceipt>();
			board.RemoveRestoreSource(receipt.transactionId);
		}

		#endregion

		public override IEnumerable<ITradeCostResultHandle> EnumerateActualResult(Tradeboard board)
		{
			this.RegisterResultHandleTo(board, out IAPConsumableTradeCostResultHandle handle);
			yield return handle;
		}
	}

	public class IAPConsumableTradeCostResult : ITradeCostResult
	{
		public ContentReference<IAPConsumableProductEntry> productRef;
	}

	public class IAPConsumableTradeCostResultHandle : TradeCostResultHandle<IAPConsumableTradeCost>
	{
		public override ITradeCostResult Bake()
		{
			return new IAPConsumableTradeCostResult
			{
				productRef = Source.product,
			};
		}
	}
}
