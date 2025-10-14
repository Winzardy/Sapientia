using System;
using Content;

namespace Trading
{
	[Serializable]
	public struct TraderOfferReference
	{
		public ContentReference<TraderConfig> trader;
		public ContentReference<TradeCatalogConfig> catalog;
		public int offerIndex;

		public readonly ref readonly TraderOfferConfig Config => ref catalog
		   .Read()
		   .offers[offerIndex];

		public TraderOfferReference(in ContentReference<TraderConfig> trader, in ContentReference<TradeCatalogConfig> catalog,
			int offerIndex)
		{
			this.trader = trader;
			this.catalog = catalog;
			this.offerIndex = offerIndex;
		}

		public override string ToString() => $"trader: {trader}, catalog: {catalog}, index: {offerIndex}";
	}
}
