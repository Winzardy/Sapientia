namespace Content
{
	public partial struct ContentReference<T>
	{
		public bool Contains() => !IsEmpty();

		public readonly bool IsEmpty() =>
			guid == IContentReference.SINGLE_GUID
				? !ContentManager.Contains<T>()
				: guid == SerializableGuid.Empty || !ContentManager.Contains<T>(in guid);
	}

	public partial struct ContentReference
	{
		public bool Contains() => !IsEmpty();

		public bool IsEmpty() => throw ContentDebug.Exception(
			"IsEmpty can only be accessed on ContentReference<T>, use IsEmpty<T>");

		public readonly bool Contains<T>() =>
			guid == IContentReference.SINGLE_GUID
				? !ContentManager.Contains<T>()
				: guid == SerializableGuid.Empty || !ContentManager.Contains<T>(in guid);
		public readonly bool IsEmpty<T>() =>
			guid == IContentReference.SINGLE_GUID
				? !ContentManager.Contains<T>()
				: guid == SerializableGuid.Empty || !ContentManager.Contains<T>(in guid);
	}
}
