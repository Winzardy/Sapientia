using System.Collections.Generic;
using Trading.Result;

namespace Trading
{
	public abstract partial class TradeReward
	{
		public virtual IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
		}

		public virtual IEnumerable<TradeReward> EnumerateActual(Tradeboard board)
		{
			yield return this;
		}

		public virtual IEnumerable<ITradeRewardResultHandle> EnumerateActualResult(Tradeboard board)
		{
			yield break;
		}
	}
}
