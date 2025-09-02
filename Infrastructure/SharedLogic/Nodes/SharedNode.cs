using System;
using Sapientia;

namespace SharedLogic
{
	public abstract class SharedNode : IInitializableNode, IDisposable
	{
		protected ISharedRoot _root;

		public virtual string Id => GetType().Name;

		internal ILogger Logger => _root.Logger;

		void IInitializableNode.Initialize(ISharedRoot root)
		{
			_root = root;

			OnInitialize(root);
		}

		public void Dispose()
		{
			OnDisposed();
			_root = null;
		}

		protected virtual void OnInitialize(ISharedRoot root)
		{
		}

		protected virtual void OnDisposed()
		{
		}
	}

	public abstract class SharedNode<TData> : SharedNode, IPersistentNode
	{
		private TData _data;

		void IPersistentNode.Load(ISharedDataReader reader)
		{
			_data = reader.Read<TData>(Id);
			OnLoad(in _data);
		}

		void IPersistentNode.Save(ISharedDataWriter writer)
		{
			OnSave(ref _data);
			writer.Write(Id, in _data);
		}

		protected abstract void OnLoad(in TData data);
		protected abstract void OnSave(ref TData data);
	}
}
