using System.Collections;
using System.Collections.Generic;

namespace Trading
{
	public sealed partial class TradeCostCollection : IEnumerable<TradeCost>
	{
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public IEnumerator<TradeCost> GetEnumerator()
		{
			foreach (var item in items)
				yield return item;
		}
	}
}
