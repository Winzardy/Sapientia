using System.Collections.Generic;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class TradeRewardProgression //: IEnumerable<TradeReward>
	{
		protected internal override IEnumerable<TradeReward> EnumerateActualInternal(Tradeboard board)
		{
			foreach (var reward in GetCurrentStage(board)
				.reward
				.EnumerateActualInternal(board))
				yield return reward;
		}
	}
}
