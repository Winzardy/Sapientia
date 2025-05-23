#if CLIENT
using System;
using Sapientia;
using Sapientia.Extensions.Reflection;
using Sirenix.OdinInspector;

namespace Trading
{
	[TypeRegistryItem(
		"\u2009Options",
		"",
		SdfIconType.UiChecksGrid,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B, darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B, lightIconColorA: A,
		priority: 6)]
	public sealed partial class TradeCostOptions
	{
		/// <summary>
		/// Фильтрует типы только в инспекторе!
		/// </summary>
		public bool Filter(Type type) => !typeof(IOptions<TradeCost>).IsAssignableFrom(type) && type.HasAttribute<SerializableAttribute>();
	}
}
#endif
