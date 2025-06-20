namespace Content
{
	public partial struct ContentReference<T>
	{
		public bool IsSingle => guid == IContentReference.SINGLE_GUID;
		public static readonly ContentReference<T> Single = new(in IContentReference.SINGLE_GUID);
	}

	public partial struct ContentReference
	{
		public bool IsSingle => guid == IContentReference.SINGLE_GUID;
		public static readonly ContentReference Single = new(in IContentReference.SINGLE_GUID);
	}
}
