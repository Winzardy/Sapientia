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
				foreach (var cost in option) // ограничение на одну вложенность в системе!
					yield return cost;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
