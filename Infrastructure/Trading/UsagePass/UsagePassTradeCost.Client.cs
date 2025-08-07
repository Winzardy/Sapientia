#if CLIENT
using Sirenix.OdinInspector;

namespace Trading.UsagePass
{
	[TypeRegistryItem(
		"\u2009Usage Pass", //В начале делаем отступ из-за отрисовки...
		"",
		SdfIconType.ShieldLock,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B, darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B, lightIconColorA: A)]
	public partial class UsagePassTradeCost
	{
	}
}
#endif
