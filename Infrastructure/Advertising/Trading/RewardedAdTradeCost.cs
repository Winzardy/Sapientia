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
		public override int Priority => TradeCostPriority.VERY_HIGH;

		public ContentReference<RewardedAdPlacementEntry> placement;

		public override bool CanPay(out TradePayError? error)
		{
			error = null;

			var success = AdManager.CanShow(placement, out var localError);

			if (localError.HasValue)
				error = new TradePayError(TradeCostCategory.ADVERTISING, (int) localError.Value.code, localError);

			return success;
		}

		protected override Task<bool> PayAsync(CancellationToken cancellationToken) => AdManager.ShowAsync(placement, cancellationToken);

		/// <summary>
		/// <para>Никто не в силах вернуть потраченное время от рекламы T_T...</para>
		/// Если только не запомнить что игрок полностью просмотрел рекламу и при следующем воспроизведении пропустить...
		/// Но есть вопросики конечно
		/// </summary>
		public override bool CanReturn(out TradeCostReturnError? error) => base.CanReturn(out error);
	}
}
