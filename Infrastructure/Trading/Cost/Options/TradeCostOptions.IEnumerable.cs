using System.Collections;
using System.Collections.Generic;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public sealed partial class TradeCostOptions : IEnumerable<TradeCost>
	{
		public new IEnumerator<TradeCost> GetEnumerator()
		{
			foreach (var option in options)
				yield return option;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
