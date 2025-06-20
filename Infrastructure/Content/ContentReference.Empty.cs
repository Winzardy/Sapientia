namespace Content
{
	public partial struct ContentReference<T>
	{
		public readonly bool Contains() => !IsEmpty() &&
			guid == IContentReference.SINGLE_GUID
				? ContentManager.Contains<T>()
				: ContentManager.Contains<T>(in guid);

		public readonly bool IsEmpty() => guid == SerializableGuid.Empty;
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
