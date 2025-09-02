#nullable enable
namespace SharedLogic
{
	public interface ISharedDataReader
	{
		public TData? Read<TData>(string key);
	}
}
