using Sapientia;

namespace Content
{
	public interface IUniqueContentEntrySource<T> : IUniqueContentEntrySource, IContentEntrySource<T>
	{
		public new IUniqueContentEntry<T> UniqueContentEntry { get; }
	}

	public interface IUniqueContentEntrySource : IContentEntrySource, IIdentifiable
	{
		public IUniqueContentEntry UniqueContentEntry { get; }
	}

	public interface IContentEntrySource<T> : IContentEntrySource
	{
		public new IContentEntry<T> ContentEntry { get; }
	}

	public interface IContentEntrySource
	{
		/// <summary>
		/// <see cref="ContentEntryScriptableObject{T}._entry"/>
		/// </summary>
		public const string ENTRY_FIELD_NAME = "_entry";

		public IContentEntry ContentEntry { get; }
		public bool Validate();
	}

	public interface INestedContentEntrySource : IContentEntrySource
	{
		//TODO: возможно имеет смысл добавить "оптимальный" способ узнать тип
		//public Type ValueType { get; }
		public IContentEntrySource Source { get; }
		public IUniqueContentEntry UniqueContentEntry { get; }
		IContentEntry IContentEntrySource.ContentEntry => UniqueContentEntry;
	}
}
