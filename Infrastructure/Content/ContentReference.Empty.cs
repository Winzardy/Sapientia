namespace Content
{
	public partial struct ContentReference<T>
	{
		public readonly bool IsValid() => Has();

		public readonly bool IsEmpty() => guid == SerializableGuid.Empty;

		private readonly bool Has() => !IsEmpty() &&
			guid == IContentReference.SINGLE_GUID
				? ContentManager.Contains<T>()
				: ContentManager.Contains<T>(in guid);
	}

	public partial struct ContentReference
	{
		public readonly bool Contains() => throw ContentDebug.Exception(
			"IsEmpty can only be accessed on ContentReference<T>, use IsEmpty<T>");

		public readonly bool IsEmpty() => guid == SerializableGuid.Empty;

		public readonly bool Contains<T>() => !IsEmpty() &&
			guid == IContentReference.SINGLE_GUID
				? ContentManager.Contains<T>()
				: ContentManager.Contains<T>(in guid);
	}
}
