#if CLIENT
using System;
using System.Collections.Generic;
using Sapientia.Extensions.Reflection;
using Sirenix.OdinInspector;

namespace Trading
{
	[TypeRegistryItem(
		"\u2009Reward By Weight",
		"Random",
		SdfIconType.Dice5Fill,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B,
		darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A)]
	public partial class WeightedTradeRewardCollection
	{
		/// <summary>
		/// Фильтрует типы только в инспекторе!
		/// </summary>
		public bool Filter(Type type) => !typeof(IEnumerable<TradeReward>).IsAssignableFrom(type) && type.HasAttribute<SerializableAttribute>();
	}
}
#endif
