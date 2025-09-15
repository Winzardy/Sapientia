using System.ComponentModel;
using SharedLogic.Internal;

namespace SharedLogic
{
	public interface IPersistentNode<TData> : IPersistentNode
	{
		void IPersistentNode.Load(ISharedDataReader reader)
		{
			var data = reader.Read<TData>(Id);
			Load(in data);
		}

		void IPersistentNode.Save(ISharedDataWriter writer)
		{
			Save(out var data);
			writer.Write(Id, in data);
		}

		public void Load(in TData data);
		public void Save(out TData data);
	}
}

namespace SharedLogic.Internal
{
	/// <summary>
	/// Для внутреннего использования, для внешнего используйте <see cref="IPersistentNode{TData}"/>
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IPersistentNode : ISharedNode
	{
		internal void Load(ISharedDataReader reader);
		internal void Save(ISharedDataWriter writer);
	}
}
