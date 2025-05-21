using System;

namespace Content
{
	[Serializable]
	public partial struct ContentReference<T> : IContentReference
	{
		public SerializableGuid guid;

		[NonSerialized]
		public int index;

		public Type ValueType => typeof(T);

		SerializableGuid IContentReference.Guid => guid;

		public ContentReference(in SerializableGuid guid, int index = IContentReference.NO_INDEX)
		{
			this.guid = guid;
			this.index = index;
		}

		public override string ToString() => ContentManager.ToLabel<T>(in guid);
		public string ToString(bool detailed) => ContentManager.ToLabel<T>(in guid, detailed);

		public override int GetHashCode() => guid.GetHashCode();
	}

	[Serializable]
	public partial struct ContentReference : IContentReference
	{
		public SerializableGuid guid;

		[NonSerialized]
		public int index;

		public SerializableGuid Guid => guid;

		Type IContentReference.ValueType => throw ContentDebug.Exception(
			"ValueType can only be accessed on ContentReference<T>." +
			"When using ContentReference without a type parameter, you must rely on implicit (“hidden”)" +
			" knowledge of the contained type");

		public ContentReference(in SerializableGuid guid, int index = IContentReference.NO_INDEX)
		{
			this.guid = guid;
			this.index = index;
		}
	}

	/// <summary>
	/// В основном используется в редакторе
	/// </summary>
	public interface IContentReference
	{
		/// <summary>
		/// Не определенный индекс
		/// </summary>
		public const int NO_INDEX = -1;

		/// <summary>
		/// <see cref="ContentReference{T}.guid"/>
		/// </summary>
		public const string GUID_FIELD_NAME = "guid";

		/// <summary>
		/// <see cref="ContentReference{T}.index"/>
		/// </summary>
		public const string INDEX_FIELD_NAME = "single";

		/// <summary>
		/// Когда <see cref="ContentReference{T}"/> ссылается на <see cref="SingleContentEntry{T}"/>
		/// </summary>
		public static readonly SerializableGuid SINGLE_GUID = new(-1, -1);

		public bool IsSingle { get; }
		public SerializableGuid Guid { get; }
		public bool IsEmpty();
		Type ValueType { get; }
	}
}
