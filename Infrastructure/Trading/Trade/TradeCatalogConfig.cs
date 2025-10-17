using System;
using Content;
using Sapientia;
using Sapientia.Collections;
using Sapientia.Conditions;

#if CLIENT
using UnityEngine;
using Sirenix.OdinInspector;
#endif

namespace Trading
{
	/// <summary>
	/// Каталог лотов, позиций
	/// </summary>
	[Serializable]
	public partial class TradeCatalogConfig : IExternallyIdentifiable
	{
		private string _id;

		public TraderOfferConfig[] offers;

		public string Id => _id;

		void IExternallyIdentifiable.SetId(string id) => _id = id;

		public RefEnumerator<TraderOfferConfig> GetEnumerator() => new(offers);
	}

	/// <remarks>
	/// Лот, позиция в магазине, не воспринимать как оффер (попап с предложением покупки), возможно название неудачное...
	/// </remarks>
	[Serializable]
	public partial struct TraderOfferConfig
	{
#if CLIENT
		[BoxGroup]
#endif
		[SerializeReference]
		public Condition<Blackboard> condition;

		public ContentReference<TradeConfig> trade;
	}
}
