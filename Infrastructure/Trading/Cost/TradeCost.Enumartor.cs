using System.Collections.Generic;
using Trading.Result;

namespace Trading
{
	public abstract partial class TradeCost
	{
		public virtual IEnumerator<TradeCost> GetEnumerator()
		{
			yield return this;
		}

		public virtual IEnumerable<TradeCost> EnumerateActual(Tradeboard board)
		{
			yield return this;
		}

		public virtual IEnumerable<ITradeCostResultHandle> EnumerateActualResult(Tradeboard board)
		{
			yield break;
		}
	}
}
