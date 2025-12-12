using System;
using System.Threading;
using System.Threading.Tasks;
using Advertising;
using Content;
using Sapientia;
using Sapientia.Evaluators;

namespace Trading.Advertising
{
	public interface IRewardedAdvertisingTradeCost
	{
		public int Group { get; }
		public int GetCount(Tradeboard board);
		public int GetCountAndRegisterResult(Tradeboard board);
	}

	[Serializable]
	public partial class RewardedAdTradeCost : TradeCostWithReceipt<AdTradeReceipt>, IRewardedAdvertisingTradeCost
	{
		private const bool USE_AUTO_LOAD = true;
		private const string ERROR_CATEGORY = "Advertising";

		public override int Priority => TradeCostPriority.VERY_HIGH;

		public ContentReference<RewardedAdPlacementEntry> placement = "Default";

		[ContextLabel(AdTradeReceipt.AD_TOKEN_LABEL_CATALOG)]
		public int group;

		public EvaluatedValue<Blackboard, int> count = 1;

		protected override bool CanFetch(Tradeboard board, out TradePayError? error)
		{
			error = null;

			if (board.Contains<IAdvertisingNode>())
			{
				var node = board.Get<IAdvertisingNode>();
				if (node.GetTokenCount(group) >= GetCountInternal(board))
					return true;
			}

			var success = AdManager.CanShow(placement, out var adError);

			if (adError.HasValue)
			{
				if (adError.Value.code == AdShowErrorCode.NotLoaded && USE_AUTO_LOAD)
					return true;

				error = new TradePayError(ERROR_CATEGORY, (int) adError.Value.code, adError);
			}

			return success;
		}

		protected override async Task<AdTradeReceipt?> FetchAsync(Tradeboard board, CancellationToken cancellationToken)
		{
			if (board.Contains<IAdvertisingNode>())
			{
				var node = board.Get<IAdvertisingNode>();
				if (node.GetTokenCount(group) >= GetCountInternal(board))
					return AdTradeReceipt.Empty(group, placement);
			}

			//TODO: может быть какую-то защиту от дурака, что нельзя посмотреть рекламу за секунду)
			var result = await AdManager.ShowAsync(placement, cancellationToken, USE_AUTO_LOAD);

			if (result == AdShowResult.Canceled)
				throw new OperationCanceledException(cancellationToken);

			if (result != AdShowResult.Success)
				return null;

			var dateTime = board.Get<IDateTimeProviderWithVirtual>()
			   .DateTimeWithoutOffset;
			return new AdTradeReceipt(group, placement, dateTime.Ticks);
		}

		private int GetCountInternal(Tradeboard board)
		{
			return count.Evaluate(board);
		}

		protected override string GetReceiptKey(string _) => placement.ToReceiptKey(group);

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
			return tradeboard.Get<IAdvertisingNode>()
			   .GetTokenCount(group);
		}

		private BlackboardToken _token;

		protected override void OnBeforePayCheck(Tradeboard board) => _token = board.Register<IRewardedAdvertisingTradeCost>(this);
		protected override void OnAfterPayCheck(Tradeboard board) => _token.Release();
		protected override void OnBeforePay(Tradeboard board) => _token = board.Register<IRewardedAdvertisingTradeCost>(this);
		protected override void OnAfterPay(Tradeboard board) => _token.Release();

		int IRewardedAdvertisingTradeCost.Group => group;

		int IRewardedAdvertisingTradeCost.GetCount(Tradeboard board)
		{
			return count.Evaluate(board);
		}
	}
}
