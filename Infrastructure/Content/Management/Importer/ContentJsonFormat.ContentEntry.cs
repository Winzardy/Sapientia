namespace Content
{
	public struct UniqueContentEntryJsonObject
	{
		public SerializableGuid guid;
		public readonly object rawValue;
		public string id;

		public UniqueContentEntryJsonObject(object rawValue, in SerializableGuid guid, string id = null)
		{
			this.guid = guid;
			this.rawValue = rawValue;
			this.id = id;
		}

		public UniqueContentEntryJsonObject(in SerializableGuid guid)
		{
			this.guid = guid;
			rawValue = null;
			id = null;
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
			_id = dto.id;
			guid = dto.guid;
#if CLIENT
			ContentEditValue
#else
			value
#endif
				= dto.rawValue is T t ? t : (T) dto.rawValue;
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
#if CLIENT
			ContentEditValue
#else
			value
#endif
				= dto.rawValue is T t ? t : (T) dto.rawValue;
		}

		public SingleContentEntry()
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
