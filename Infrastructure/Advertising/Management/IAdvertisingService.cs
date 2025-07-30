namespace Advertising
{
	public interface IAdvertisingService
	{
		public bool CanShow(AdPlacementKey key, out AdShowError? error);
		public void RegisterShow(AdPlacementKey key);
	}

	public interface IAdvertisingServiceFactory
	{
		IAdvertisingService Create();
	}
}
