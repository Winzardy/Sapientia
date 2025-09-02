#nullable enable
namespace SharedLogic
{
	public interface ISharedData
	{
		public void Load(string json);
		public string Save();
	}
}
