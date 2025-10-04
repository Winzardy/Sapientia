using System;
using Content;

namespace Trading
{
	[Constants]
	[Serializable]
	public partial struct TraderConfig
	{
		public ContentReference<TradeCatalogConfig>[] catalogs;
	}
}
