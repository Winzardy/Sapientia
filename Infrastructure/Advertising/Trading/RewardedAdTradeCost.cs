using System;
using System.Threading;
using System.Threading.Tasks;
using Advertising;
using Content;

namespace Trading.Advertising
{
	[Serializable]
	public partial class RewardedAdTradeCost : TradeCostWithReceipt<AdTradeReceipt>
	{
		private const string ERROR_CATEGORY = "Advertising";

		public override int Priority => TradeCostPriority.VERY_HIGH;

		public ContentReference<RewardedAdPlacementEntry> placement;
		public int count = 1;
		protected override int ReceiptCount => count;

		protected override string ReceiptId => placement.ToReceiptId();

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;

			var success = AdManager.CanShow(placement, out var localError);

			if (localError.HasValue)
				error = new TradePayError(ERROR_CATEGORY, (int) localError.Value.code, localError);

			return success;
		}

		protected override async Task<AdTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			//TODO: может быть какую-то защиту от дурака, что нельзя посмотреть рекламу за секунду)
			var success = await AdManager.ShowAsync(placement, cancellationToken);

			if (!success)
				return null;

			return new AdTradeReceipt(placement);
		}

		/// <summary>
		/// <para>Никто не в силах вернуть потраченное время от рекламы T_T...</para>
		/// Если только не запомнить что игрок полностью просмотрел рекламу и при следующем воспроизведении пропустить...
		/// Но есть вопросики конечно
		/// </summary>
		protected override bool CanRefund(Tradeboard board, out TradeCostRefundError? error)
		{
			error = null;
			return false;
		}
	}
}

