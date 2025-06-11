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
			//TODO: тут надо проверять смотрели ли рекламу)
			return true;
		}

		protected override bool Pay(Tradeboard board)
		{
			//TODO израсходовать квитанцию за просмотр рекламы
			return true;
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
