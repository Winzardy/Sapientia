namespace SharedLogic
{
	public partial interface ISharedNode
	{
		public string Id { get; }
	}

	public interface IInitializableNode : ISharedNode
	{
		void Initialize(ISharedRoot root);
	}
}
