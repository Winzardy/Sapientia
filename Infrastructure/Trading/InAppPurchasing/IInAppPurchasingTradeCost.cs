using InAppPurchasing;
using JetBrains.Annotations;

namespace Trading.InAppPurchasing
{
	/// <summary>
	/// Обязательно нужно вешать на цену, чтобы вспомогательные системы смогли
	/// получить продукт, например TraderPurchaseGranter
	/// </summary>
	public interface IInAppPurchasingTradeCost
	{
		[CanBeNull]
		public IAPProductEntry GetProductEntry();
	}
}
