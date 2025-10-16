using System.Collections.Generic;

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
	}
}
