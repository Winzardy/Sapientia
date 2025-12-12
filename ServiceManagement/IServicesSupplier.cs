using System.Collections.Generic;

namespace Game.App.ServiceManagement
{
	public interface IServicesSupplier
	{
		T Get<T>();
		bool TryGet<T>(out T service);
	}
}
