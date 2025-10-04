#nullable enable
namespace SharedLogic
{
	/// <summary>
	/// Провайдер для работы с сохраняемой датой!
	/// </summary>
	/// <remarks>
	///	Назвал Manipulator специально, потому что оно странное) и хорошо выделяется
	/// </remarks>
	public interface ISharedDataManipulator : ISharedDataReader, ISharedDataWriter
	{
		public void Load(string json);
		public string Save();

		/// <summary>
		/// Специфичная версия даты, нужна лишь чтобы трекать "были ли изменения"?
		/// </summary>
		public int Revision { get; }
	}

	public interface ISharedDataManipulatorFactory
	{
		public ISharedDataManipulator Create(ISharedRoot root);
	}

	public interface ISharedDataReader
	{
		public TData? Read<TData>(string key);
	}

	public interface ISharedDataWriter
	{
		public void Write<TData>(string key, in TData saveData);
	}
}
