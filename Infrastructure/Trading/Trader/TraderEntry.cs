using System;
using Content;

namespace Trading
{
	[Constants]
	[Serializable]
	public partial struct TraderEntry
	{
		public ContentReference<TradeCatalogEntry>[] catalogs;
	}
}
