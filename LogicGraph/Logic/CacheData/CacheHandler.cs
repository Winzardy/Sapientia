using Sapientia.Data;

namespace Sapientia.LogicGraph
{
	public struct CacheHandler<T> where T : unmanaged
	{
		public PtrOffset<DataCache<T>> offset;
	}
}
