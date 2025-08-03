using System;
using Content;
using Sapientia;
using Sapientia.Collections;

namespace Trading
{
	/// <summary>
	/// Каталог лотов, позиций
	/// </summary>
	[Serializable]
	public partial class TradeCatalogEntry : IExternallyIdentifiable
	{
		private string _id;

		public TraderOfferEntry[] offers;

		public string Id => _id;

		void IExternallyIdentifiable.SetId(string id)
		{
			_id = id;
		}

		public RefEnumerator<TraderOfferEntry> GetEnumerator() => new(offers);
	}

	/// <remarks>
	/// Лот, позиция в магазине, не воспринимать как оффер (попап с предложением покупки), возможно название неудачное...
	/// </remarks>
	[Serializable]
	public partial struct TraderOfferEntry
	{
		//TODO: добавить прогрессивную сделку, проблема что еще эту прогрессию где-то надо фиксировать
		public ContentReference<TradeEntry> trade;
		public bool checkPurchased;
	}
}
