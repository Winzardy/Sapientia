#if CLIENT
using System;
using System.Collections.Generic;
using Sapientia.Extensions.Reflection;
using Sirenix.OdinInspector;

namespace Trading
{
	[TypeRegistryItem(
		"\u2009Collection",
		"",
		SdfIconType.Basket,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A)]
	public partial class TradeRewardCollection
	{
		/// <summary>
		/// Фильтрует типы только в инспекторе!
		/// </summary>
		public bool Filter(Type type) => !typeof(IEnumerable<TradeReward>).IsAssignableFrom(type) && type.HasAttribute<SerializableAttribute>();
	}
}
#endif
