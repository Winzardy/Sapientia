using System;
using System.ComponentModel;
using SharedLogic.Internal;

namespace SharedLogic
{
	public interface IPersistentNode<TData> : IPersistentNode
	{
		Type IPersistentNode.DataType => typeof(TData);

		void IPersistentNode.Load(ISharedDataStreamer streamer)
		{
			var data = streamer.Read<TData>(Id);
			Load(in data);
		}

		void IPersistentNode.Save(ISharedDataStreamer streamer)
		{
			Save(out var data);
			streamer.Write(Id, in data);
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
		public Type DataType { get; }
		internal void Load(ISharedDataStreamer streamer);
		internal void Save(ISharedDataStreamer streamer);
		string Id { get; }
	}
}
