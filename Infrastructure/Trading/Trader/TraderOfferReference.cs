using System;
using Content;

namespace Trading
{
	[Serializable]
	public struct TraderOfferReference
	{
		public ContentReference<TraderEntry> trader;
		public ContentReference<TradeCatalogEntry> catalog;
		public int offerIndex;

		public readonly ref readonly TraderOfferEntry GetEntry() => ref catalog
		   .Read()
		   .offers[offerIndex];

		public TraderOfferReference(in ContentReference<TraderEntry> trader, in ContentReference<TradeCatalogEntry> catalog, int offerIndex)
		{
			this.trader = trader;
			this.catalog = catalog;
			this.offerIndex = offerIndex;
		}

		public override string ToString() => $"trader: {trader}, catalog: {catalog}, index: {offerIndex}";
	}
}
