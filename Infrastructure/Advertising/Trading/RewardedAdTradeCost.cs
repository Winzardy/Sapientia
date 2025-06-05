using System;
using System.Threading;
using System.Threading.Tasks;
using Advertising;
using Content;

namespace Trading.Advertising
{
	[Serializable]
	public partial class RewardedAdTradeCost : TradeCost
	{
		private const string ERROR_CATEGORY = "Advertising";

		public override int Priority => TradeCostPriority.VERY_HIGH;

		public ContentReference<RewardedAdPlacementEntry> placement;

		protected override bool CanPay(Tradeboard board, out TradePayError? error)
		{
			error = null;

			var success = AdManager.CanShow(placement, out var localError);

			if (localError.HasValue)
				error = new TradePayError(ERROR_CATEGORY, (int) localError.Value.code, localError);

			return success;
		}

		protected override Task<bool> PayAsync(Tradeboard board, CancellationToken cancellationToken) =>
			AdManager.ShowAsync(placement, cancellationToken);

		/// <summary>
		/// <para>Никто не в силах вернуть потраченное время от рекламы T_T...</para>
		/// Если только не запомнить что игрок полностью просмотрел рекламу и при следующем воспроизведении пропустить...
		/// Но есть вопросики конечно
		/// </summary>
		public override bool CanReturn(Tradeboard board, out TradeCostReturnError? error)
			=> base.CanReturn(board, out error);
	}
}
