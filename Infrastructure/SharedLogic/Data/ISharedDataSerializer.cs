#nullable enable
namespace SharedLogic
{
	public interface ISharedDataSerializer
	{
		public void Load(string json);
		public string Save();
	}

	public interface ISharedDataSerializerFactory
	{
		public ISharedDataSerializer Create(ISharedRoot root);
	}
}
