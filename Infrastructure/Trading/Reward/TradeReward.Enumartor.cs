using System.Collections.Generic;

namespace Trading
{
	public abstract partial class TradeReward
	{
		public virtual IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
		}

		protected internal virtual IEnumerable<TradeReward> EnumerateActualInternal(Tradeboard board)
		{
			yield return this;
		}
	}
}
