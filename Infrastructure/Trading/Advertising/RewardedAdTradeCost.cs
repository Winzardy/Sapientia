using System;
using System.Threading;
using System.Threading.Tasks;
using Advertising;
using Content;
using Sapientia;

namespace Trading.Advertising
{
	[Serializable]
	public partial class RewardedAdTradeCost : TradeCostWithReceipt<AdTradeReceipt>
	{
		private const string ERROR_CATEGORY = "Advertising";

		public override int Priority => TradeCostPriority.VERY_HIGH;

		public ContentReference<RewardedAdPlacementEntry> placement;
		[ContextLabel(AdTradeReceipt.AD_TOKEN_LABEL_CATALOG)]
		public int group;
		public int count = 1;

		protected override string ReceiptId => placement.ToReceiptId(group);

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;

			if (board.Contains<IAdvertisingBackend>())
			{
				var model = board.Get<IAdvertisingBackend>();
				if (model.GetTokenCount(group) >= count)
					return true;
			}

			var success = AdManager.CanShow(placement, out var adError);

			if (adError.HasValue)
			{
				if (adError.Value.code == AdShowErrorCode.NotLoaded) // TODO: временный фикс, недоступности рекламы
					return true;

				error = new TradePayError(ERROR_CATEGORY, (int) adError.Value.code, adError);
			}

			return success;
		}

		protected override async Task<AdTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			if (board.Contains<IAdvertisingBackend>())
			{
				var model = board.Get<IAdvertisingBackend>();
				if (model.GetTokenCount(group) >= count)
					return AdTradeReceipt.Empty(group, placement);
			}

			//TODO: может быть какую-то защиту от дурака, что нельзя посмотреть рекламу за секунду)
			var success = await AdManager.ShowAsync(placement, cancellationToken);

			if (!success)
				return null;

			var dateTime = board.Get<IDateTimeProvider>().Now;
			return new AdTradeReceipt(group, placement, dateTime.Ticks);
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

		public int GetAvailableCount(Tradeboard tradeboard)
		{
			return tradeboard.Get<IAdvertisingBackend>()
			   .GetTokenCount(group);
		}

		private IBlackboardToken _token;

		protected override void OnBeforePayCheck(Tradeboard board) => _token = board.Register(this);
		protected override void OnAfterPayCheck(Tradeboard board) => _token.Release();
		protected override void OnBeforePay(Tradeboard board) => _token = board.Register(this);
		protected override void OnAfterPay(Tradeboard board) => _token.Release();
	}
}
