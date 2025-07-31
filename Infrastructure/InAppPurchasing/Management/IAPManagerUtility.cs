using Content;

namespace InAppPurchasing
{
	public static class IAPManagerUtility
	{
		public static ref readonly ProductInfo ToProductInfo<T>(this in ContentReference<T> entry, bool forceUpdateCache = false)
			where T : IAPProductEntry
			=> ref IAPManager.GetProductInfo<T>(entry, forceUpdateCache);
	}
}
