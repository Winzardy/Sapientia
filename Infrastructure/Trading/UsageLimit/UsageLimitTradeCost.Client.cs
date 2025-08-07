#if CLIENT
using Sirenix.OdinInspector;

namespace Trading.UsageLimit
{
	[TypeRegistryItem(
		"\u2009Usage Limit", //В начале делаем отступ из-за отрисовки...
		"",
		SdfIconType.ShieldLockFill,
		darkIconColorR: R, darkIconColorG: G, darkIconColorB: B, darkIconColorA: A,
		lightIconColorR: R, lightIconColorG: G, lightIconColorB: B, lightIconColorA: A)]
	public partial class UsageLimitTradeCost
	{
	}
}
#endif
