using System.Collections;
using System.Collections.Generic;
using Trading.Result;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public partial class TradeRewardProgression : IEnumerable<TradeReward>
	{
		public IEnumerator<TradeReward> GetEnumerator()
		{
			foreach (var item in stages.Value)
				yield return item.reward;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		protected internal override IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			foreach (var reward in GetCurrentStage(board)
				        .reward
				        .EnumerateActual(board))
				yield return reward;
		}
	}
}
