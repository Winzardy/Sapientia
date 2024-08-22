namespace Sapientia.TypeIndexer
{
	public struct ProxyIndex
	{
		internal int index;

		public static implicit operator int(ProxyIndex typeIndex)
		{
			return typeIndex.index;
		}

		public static implicit operator ProxyIndex(int index)
		{
			return new ProxyIndex{ index = index, };
		}
	}
}
