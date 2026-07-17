using Sapientia;

namespace Content
{
	public interface IUniqueContentEntrySource<T> : IUniqueContentEntrySource, IContentEntrySource<T>
	{
		new IUniqueContentEntry<T> UniqueContentEntry { get; }
	}

	public interface IUniqueContentEntrySource : IContentEntrySource, IIdentifiable
	{
		IUniqueContentEntry UniqueContentEntry { get; }

		long CreationOrder { get => long.MaxValue; }

		ref readonly SerializableGuid Guid { get => ref UniqueContentEntry.Guid; }
	}

	public interface IContentEntrySource<T> : IContentEntrySource
	{
		new IContentEntry<T> ContentEntry { get; }
	}

	public partial interface IContentEntrySource
	{
		/// <summary>
		/// <see cref="ContentEntryScriptableObject{T}._entry"/>
		/// </summary>
		const string ENTRY_FIELD_NAME = "_entry";

		bool enabled { get; }
		IContentEntry ContentEntry { get; }
		bool Validate();
	}

	public interface INestedContentEntrySource : IContentEntrySource
	{
		//TODO: возможно имеет смысл добавить "оптимальный" способ узнать тип
		//public Type ValueType { get; }
		IContentEntrySource Source { get; }
		IUniqueContentEntry UniqueContentEntry { get; }
		bool IContentEntrySource.enabled => Source.enabled;
		IContentEntry IContentEntrySource.ContentEntry => UniqueContentEntry;
	}
}
