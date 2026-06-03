using System.Collections;
using System.Collections.Generic;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject]
#endif
	public abstract partial class TradeReward : IEnumerable<TradeReward>
	{
		public virtual IEnumerator<TradeReward> GetEnumerator()
		{
			yield return this;
		}

		protected internal virtual IEnumerable<TradeReward> EnumerateActualInternal(Tradeboard board)
		{
			yield return this;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
