using System;
using System.ComponentModel;
using SharedLogic.Internal;

namespace SharedLogic
{
	public interface IPersistentNode<TData> : IPersistentNode
	{
		Type IPersistentNode.DataType => typeof(TData);

		void IPersistentNode.Load(ISharedDataManipulator manipulator)
		{
			var data = manipulator.Read<TData>(Id);
			Load(in data);
		}

		void IPersistentNode.Save(ISharedDataManipulator manipulator)
		{
			Save(out var data);
			manipulator.Write(Id, in data);
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
		internal void Load(ISharedDataManipulator manipulator);
		internal void Save(ISharedDataManipulator manipulator);
		string Id { get; }
	}
}
