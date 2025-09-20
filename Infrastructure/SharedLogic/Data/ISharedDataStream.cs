#nullable enable
namespace SharedLogic
{
	public interface ISharedDataStream : ISharedDataWriter, ISharedDataReader
	{
		public void Load(string json);
		public string Save();
	}
}
