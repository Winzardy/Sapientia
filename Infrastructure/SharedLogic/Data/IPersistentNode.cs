namespace SharedLogic
{
	public interface IPersistentNode : ISharedNode
	{
		void Load(ISharedDataReader reader);
		void Save(ISharedDataWriter writer);
	}
}
