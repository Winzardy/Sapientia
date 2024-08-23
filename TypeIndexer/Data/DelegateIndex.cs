namespace Sapientia.TypeIndexer
{
	public struct DelegateIndex
	{
		internal int index;

		public static implicit operator int(DelegateIndex typeIndex)
		{
			return typeIndex.index;
		}

		public static implicit operator DelegateIndex(int index)
		{
			return new DelegateIndex{ index = index, };
		}
	}
}
