using System.Collections;
using System.Collections.Generic;

namespace Trading
{
#if NEWTONSOFT
	[Newtonsoft.Json.JsonObject]
#endif
	public abstract partial class TradeCost : IEnumerable<TradeCost>
	{
		public virtual IEnumerator<TradeCost> GetEnumerator()
		{
			yield return this;
		}

		/// <summary>
		/// Перебирает актуальный список цен, формируемый по условиям (на момент вызова метода)
		/// </summary>
		protected internal virtual IEnumerable<TradeCost> EnumerateActualInternal(Tradeboard board)
		{
			yield return this;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
