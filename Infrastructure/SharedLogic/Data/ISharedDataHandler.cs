#nullable enable
namespace SharedLogic
{
	public interface ISharedDataHandler
	{
		public void LoadJson(string json);
		public string SaveJson();
	}
}
