using Sapientia;

namespace Game.App.ServiceManagement
{
	public interface IServicesSupplier : IObjectsProvider
	{
		bool TryGet<T>(out T service);
	}
}
