using System.Collections;
using System.Collections.Generic;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject] // иначе пытается сериализовать как список
#endif
	public sealed partial class TradeCostCollection : IEnumerable<TradeCost>
	{
		public override IEnumerator<TradeCost> GetEnumerator()
		{
			foreach (var item in items)
				foreach (var cost in item) // ограничение на одну вложенность в системе!
					yield return cost;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
