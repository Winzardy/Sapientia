using System.Collections.Generic;

namespace Trading
{
	public partial class TradeRewardProgression
	{
		protected internal override IEnumerable<TradeReward> EnumerateActualInternal(Tradeboard board)
		{
			foreach (var reward in GetCurrentStage(board)
				.reward
				.EnumerateActualInternal(board))
				yield return reward;
		}

		public override IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
			for (int i = 0; i < stages.Length; i++)
				yield return stages[i].reward;
		}
	}
}
