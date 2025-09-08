#nullable enable
namespace SharedLogic
{
	public interface ISharedDataWriter
	{
		public void Write<TData>(string key, in TData saveData);
	}
}
