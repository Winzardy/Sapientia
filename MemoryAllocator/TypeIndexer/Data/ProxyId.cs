namespace Sapientia.TypeIndexer
{
	public struct ProxyId
	{
		internal int id;

		public static implicit operator int(ProxyId typeId)
		{
			return typeId.id;
		}

		public static implicit operator ProxyId(int index)
		{
			return new ProxyId{ id = index, };
		}
	}
}
