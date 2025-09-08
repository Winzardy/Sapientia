#nullable enable
namespace SharedLogic
{
	public interface ISharedDataStream
	{
		public void Load(string json);
		public string Save();
	}
}
