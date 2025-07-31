namespace InAppPurchasing
{
	/// <summary>
	/// Проектно-ориентированный интерфейс, адаптирующий <see cref="InAppPurchasing"/> под инфраструктуру и ограничения самого продукта (проекта)
	/// </summary>
	public interface IInAppPurchasingService
	{
		public bool Contains(string transactionId);
		public void Register(string transactionId, PurchaseReceipt receipt);
		PurchaseReceipt? GetReceipt(string transactionId);

		public string[] GetAll(string transactionId);
	}

	public interface IInAppPurchasingServiceFactory
	{
		IInAppPurchasingService Create();
	}
}
