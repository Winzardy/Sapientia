using System;
using Content;
using Sapientia;

namespace Trading
{
	/// <summary>
	/// Каталог лотов, позиций
	/// </summary>
	[Serializable]
	public struct TradeCatalogEntry : IExternallyIdentifiable
	{
		private string _id;

		public TradeOfferEntry[] offers;

		public string Id => _id;

		void IExternallyIdentifiable.SetId(string id)
		{
			_id = id;
		}
	}

	/// <remarks>
	/// Лот, позиция в магазине, не воспринимать как оффер (попап с предложением покупки), возможно название неудачное...
	/// </remarks>
	[Serializable]
	public struct TradeOfferEntry
	{
		public ContentReference<TradeEntry> trade;
	}
}
