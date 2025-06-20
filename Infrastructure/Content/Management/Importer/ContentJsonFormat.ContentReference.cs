namespace Content
{
	public partial struct ContentReference<T>
	{
		public ContentReference(SerializableGuid guid)
		{
			this.guid = guid;
			index = ContentConstants.INVALID_INDEX;
		}
	}

	public partial struct ContentReference
	{
		public ContentReference(SerializableGuid guid)
		{
			this.guid = guid;
			index = ContentConstants.INVALID_INDEX;
		}
	}
}
