using System;
using Content;

namespace Trading
{
	[Serializable]
	public partial struct TraderEntry
	{
		public ContentReference<TradeCatalogEntry>[] catalogs;
	}
}
