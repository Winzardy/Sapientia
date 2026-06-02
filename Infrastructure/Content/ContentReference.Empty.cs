namespace Content
{
	public partial struct ContentReference<T>
	{
		public readonly bool IsValid() => Exists();
		public readonly bool IsEmpty() => guid == SerializableGuid.Empty;
		private readonly bool Exists()
		{
			if (IsEmpty())
				return false;

			return guid == IContentReference.SINGLE_GUID
					? ContentManager.Contains<T>()
					: ContentManager.Contains<T>(in guid);
		}
	}

	public partial struct ContentReference
	{
		public readonly bool Contains() => throw ContentDebug.Exception("IsEmpty can only be accessed on ContentReference<T>, use IsEmpty<T>");
		public readonly bool IsEmpty() => guid == SerializableGuid.Empty;
		public readonly bool IsValid() => true;
		public readonly bool Exists<T>()
		{
			if (IsEmpty())
				return false;

			return guid == IContentReference.SINGLE_GUID
					? ContentManager.Contains<T>()
					: ContentManager.Contains<T>(in guid);
		}
	}
}
