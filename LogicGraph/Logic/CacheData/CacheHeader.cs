using Sapientia.Data;
using Sapientia.Extensions;
using Sapientia.Memory;

namespace Sapientia.LogicGraph
{
	// В кеше лежат данные Input/Output для нод инстанса.
	public struct CacheHeader
	{
		public RelativePtr dataCache;

		public SafePtr<DataCache<T>> GetCachePtr<T>(CacheHandler<T> handler) where T: unmanaged
		{
			return dataCache.GetPtr(handler.offset);
		}

		public ref DataCache<T> GetCache<T>(CacheHandler<T> handler) where T: unmanaged
		{
			return ref GetCachePtr(handler).Value();
		}
	}
}
