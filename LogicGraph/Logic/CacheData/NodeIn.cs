using Sapientia.Data;

namespace Sapientia.LogicGraph
{
	public struct NodeIn<T> where T : unmanaged
	{
		public CacheHandler<T> input;
	}
}
