using System;
using Sapientia;
using Sapientia.Extensions;

namespace SharedLogic
{
	public abstract class SharedNode : IInitializableNode, IDisposable
	{
		protected ISharedRoot _root;

		private string _id;

		public virtual string Id => _id ??= GetType().Name
			.Remove(nameof(SharedNode));

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

	public abstract class SharedNode<TData> : SharedNode, IPersistentNode<TData>
	{
		void IPersistentNode<TData>.Load(in TData data) => OnLoad(in data);
		void IPersistentNode<TData>.Save(out TData data) => OnSave(out data);

		/// <summary>
		/// OnInitialize вызывается раньше, чем OnLoad
		/// Так же OnLoad всегда вызывается с (default) датой если ее нет
		/// </summary>
		protected abstract void OnLoad(in TData data);
		protected abstract void OnSave(out TData data);
	}
}
