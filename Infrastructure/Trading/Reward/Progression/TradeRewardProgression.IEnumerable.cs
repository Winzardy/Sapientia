using System.Collections.Generic;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class TradeRewardProgression //: IEnumerable<TradeReward>
	{
		protected internal override IEnumerable<TradeReward> OnEnumerateActual(Tradeboard board)
		{
			foreach (var reward in GetCurrentStage(board)
				.reward
				.OnEnumerateActual(board))
				yield return reward;
		}
	}
}
