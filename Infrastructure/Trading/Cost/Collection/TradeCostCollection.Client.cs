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
		SdfIconType.Stack,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B, darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B,
		lightIconColorA: A,
		priority: 5)]
	public sealed partial class TradeCostCollection
	{
		/// <summary>
		/// Фильтрует типы только в инспекторе!
		/// </summary>
		public bool Filter(Type type) => !typeof(IEnumerable<TradeCost>).IsAssignableFrom(type) && type.HasAttribute<SerializableAttribute>();
	}
}
#endif
