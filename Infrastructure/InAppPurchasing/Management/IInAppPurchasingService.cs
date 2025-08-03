using System;

namespace InAppPurchasing
{
	/// <summary>
	/// Проектно-ориентированный интерфейс, адаптирующий <see cref="InAppPurchasing"/> под инфраструктуру и ограничения самого продукта (проекта)
	/// </summary>
	public interface IInAppPurchasingService
	{
		public void Initialize();
		public bool Contains(string transactionId);
		public InAppPurchasingRegisterResult Register(string transactionId, PurchaseReceipt receipt);
		PurchaseReceipt? GetReceipt(string transactionId);

		public string[] GetAllTransactions();
		DateTime GetUtcNow();
	}

	public enum InAppPurchasingRegisterResult
	{
		Done,
		Pending, // Interop
		// Failed
	}

	public interface IInAppPurchasingServiceFactory
	{
		IInAppPurchasingService Create();
	}
}
