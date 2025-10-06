namespace Content
{
	public struct ContentReferenceArrayElement<T>
	{
		private ContentReference<T[]> _array;
		private int _index;

		public ContentReferenceArrayElement(ContentReference<T[]> array, int index)
		{
			_array = array;
			_index = index;
		}

		public ref readonly T Get()
		{
			return ref _array.Get()[_index];
		}
	}
}
