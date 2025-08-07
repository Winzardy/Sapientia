using System;
using System.Threading;
using System.Threading.Tasks;
using Sapientia;

namespace Trading.UsageLimit
{
	[Serializable]
	public partial class UsageLimitTradeCost : TradeCostWithReceipt<UsageLimitTradeReceipt>
	{
		public const string ERROR_CATEGORY = "UsageLimit";

		public UsageLimitEntry usageLimit;

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;

			var backend = board.Get<IUsageLimitBackend>();
			var key = GetReceiptKey(board.Id);
			ref readonly var model = ref backend.GetModel(key);
			var now = board.Get<IDateTimeProvider>().Now;

			if (!usageLimit.CanApplyUsage(in model, now, out var limitApplyError))
			{
				if (board.IsRestored())
					return true;

				error = new TradePayError(ERROR_CATEGORY, 0, limitApplyError);
				return false;
			}

			return true;
		}

		protected override Task<UsageLimitTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			var dateTime = board.Get<IDateTimeProvider>().Now;
			UsageLimitTradeReceipt? receipt = new UsageLimitTradeReceipt(dateTime.Ticks);
			return Task.FromResult(receipt);
		}

		protected override string GetReceiptKey(string tradeId) => UsageLimitTradeReceiptUtility.ToRecipeKey(tradeId);

		protected override bool CanRefund(Tradeboard board, out TradeCostRefundError? error)
		{
			error = null;
			return true;
		}

		private IBlackboardToken _token;

		protected override void OnBeforePayCheck(Tradeboard board) => _token = board.Register(this);
		protected override void OnAfterPayCheck(Tradeboard board) => _token.Release();
		protected override void OnBeforePay(Tradeboard board) => _token = board.Register(this);
		protected override void OnAfterPay(Tradeboard board) => _token.Release();
	}
}
