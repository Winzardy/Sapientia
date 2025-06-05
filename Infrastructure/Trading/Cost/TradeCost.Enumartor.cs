using System.Collections.Generic;

namespace Trading
{
	public abstract partial class TradeCost
	{
		public virtual IEnumerator<TradeCost> GetEnumerator()
		{
			switch (this)
			{
				case TradeCostCollection collection:
				{
					foreach (var item in collection)
						yield return item;
					break;
				}
				case TradeCostOptions options:
				{
					foreach (var item in options)
						yield return item;
					break;
				}
				default:
					yield return this;
					break;
			}
		}
	}
}
