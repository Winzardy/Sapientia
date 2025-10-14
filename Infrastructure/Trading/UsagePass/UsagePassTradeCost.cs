using System;
using System.Threading;
using System.Threading.Tasks;
using Sapientia;

namespace Trading.UsagePass
{
	[Serializable]
	public partial class UsagePassTradeCost : TradeCostWithReceipt<UsagePassTradeReceipt>, IResettableCost
	{
		public const string ERROR_CATEGORY = "UsagePass";

		public UsageLimitScheme limit;

		public void Reset(Tradeboard board)
		{
			var backend = board.Get<IUsagePassNode>();
			var recipeKey = GetReceiptKey(board.Id);
			backend.ForceReset(recipeKey);
		}

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;

			var backend = board.Get<IUsagePassNode>();
			var recipeKey = GetReceiptKey(board.Id);
			ref readonly var model = ref backend.GetModel(recipeKey);
			var tradingNode = board.Get<ITradingNode>();
			var dateTime = tradingNode.DateTime;

			if (!limit.CanApplyUsage(in model, dateTime, out var limitApplyError))
			{
				if (board.IsRestoreState)
					return true;

				error = new TradePayError(ERROR_CATEGORY, 0, limitApplyError);
				return false;
			}

			return true;
		}

		protected override Task<UsagePassTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			var tradingNode = board.Get<ITradingNode>();
			var dateTime = tradingNode.DateTime;
			UsagePassTradeReceipt? receipt = new UsagePassTradeReceipt(dateTime.Ticks);
			return Task.FromResult(receipt);
		}

		protected override string GetReceiptKey(string tradeId) => UsagePassTradeReceiptUtility.ToRecipeKey(tradeId);

		protected override bool CanRefund(Tradeboard board, out TradeCostRefundError? error)
		{
			error = null;
			return true;
		}

		private BlackboardToken _token;

		protected override void OnBeforePayCheck(Tradeboard board) => _token = board.Register(this);
		protected override void OnAfterPayCheck(Tradeboard board) => _token.Release();
		protected override void OnBeforePay(Tradeboard board) => _token = board.Register(this);
		protected override void OnAfterPay(Tradeboard board) => _token.Release();
	}
}
