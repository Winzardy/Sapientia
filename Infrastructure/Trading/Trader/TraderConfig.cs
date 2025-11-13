using System;
using Content;

namespace Trading
{
	//TODO: перенести в Interop
	[Constants]
	[Serializable]
	public partial struct TraderConfig
	{
		public ContentReference<TradeCatalogConfig>[] catalogs;
	}
}
