namespace Content
{
	public struct UniqueContentEntryJsonObject
	{
		public SerializableGuid guid;
		public readonly object rawValue;

		public UniqueContentEntryJsonObject(object rawValue, in SerializableGuid guid)
		{
			this.guid = guid;
			this.rawValue = rawValue;
		}

		public UniqueContentEntryJsonObject(in SerializableGuid guid)
		{
			this.guid = guid;
			rawValue = null;
		}
	}

	public struct ContentEntryJsonObject
	{
		public readonly object rawValue;

		public ContentEntryJsonObject(object rawValue)
		{
			this.rawValue = rawValue;
		}
	}

	public partial class UniqueContentEntry<T>
	{
		// ReSharper disable once PossibleInvalidCastException
		void IUniqueContentEntry.Setup(in UniqueContentEntryJsonObject dto)
		{
			guid = dto.guid;
			ContentEditValue = dto.rawValue is T t ? t : (T) dto.rawValue;
		}

		public UniqueContentEntry()
		{
		}
	}

	public partial class SingleContentEntry<T>
	{
		// ReSharper disable once PossibleInvalidCastException
		void ISingleContentEntry.Setup(in ContentEntryJsonObject dto)
		{
			ContentEditValue = dto.rawValue is T t ? t : (T) dto.rawValue;
		}

		public SingleContentEntry()
		{
		}
	}

	public partial class ContentEntry<T>
	{
		public ContentEntry()
		{
		}
	}

	public partial class BaseContentEntry<T>
	{
		// ReSharper disable once PublicConstructorInAbstractClass
		public BaseContentEntry()
		{
		}
	}

	public partial interface IUniqueContentEntry
	{
		internal void Setup(in UniqueContentEntryJsonObject dto);
	}

	public partial interface ISingleContentEntry
	{
		internal void Setup(in ContentEntryJsonObject dto);
	}
}
