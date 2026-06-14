using Sapientia.Data;

namespace Sapientia.LogicGraph
{
	public struct NodeOut<T> where T : unmanaged
	{
		public CacheHandler<T> output;
	}
}
