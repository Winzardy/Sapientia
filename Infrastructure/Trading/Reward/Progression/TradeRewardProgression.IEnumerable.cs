using System.Collections.Generic;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class TradeRewardProgression //: IEnumerable<TradeReward>
	{
		protected internal override IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			foreach (var reward in GetCurrentStage(board)
				.reward
				.EnumerateActual(board))
				yield return reward;
		}
	}
}
