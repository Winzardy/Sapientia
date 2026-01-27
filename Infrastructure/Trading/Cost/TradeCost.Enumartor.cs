using System.Collections.Generic;

namespace Trading
{
	public abstract partial class TradeCost
	{
		public virtual IEnumerator<TradeCost> GetEnumerator()
		{
			yield return this;
		}

		/// <summary>
		/// Перебирает актуальный список цен, формируемый по условиям (на момент вызова метода)
		/// </summary>
		protected internal virtual IEnumerable<TradeCost> EnumerateActual(Tradeboard board)
		{
			yield return this;
		}
	}
}
