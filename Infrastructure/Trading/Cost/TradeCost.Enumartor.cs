using System.Collections.Generic;

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
	}
}
