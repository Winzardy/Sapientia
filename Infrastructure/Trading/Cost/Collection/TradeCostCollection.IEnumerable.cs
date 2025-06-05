using System.Collections;
using System.Collections.Generic;

namespace Trading
{
	public sealed partial class TradeCostCollection : IEnumerable<TradeCost>
	{
		public override IEnumerator<TradeCost> GetEnumerator()
		{
			foreach (var item in items)
				yield return item;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
