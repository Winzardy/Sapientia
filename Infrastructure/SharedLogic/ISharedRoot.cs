#nullable enable
using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sapientia;

namespace SharedLogic
{
	public interface ISharedRoot : IDisposable, IEnumerable<ISharedNode>
	{
		[CanBeNull] ILogger Logger { get; }

		public T GetNode<T>() where T : class, ISharedNode;
		public ISharedNode GetNode(string id);
		public bool TryGetNode(string id, out ISharedNode node);

		public void Initialize();
		public void Save(ISharedDataStreamer streamer);
		public void Load(ISharedDataStreamer streamer);

		public IEnumerable<TFilter> Enumerate<TFilter>();

		public int Revision { get; }

		public void OnExecuted<T>(in T command)
			where T : struct, ICommand;
	}

	public static class SharedRootUtility
	{
		public static void GetNode<T>(this ISharedRoot root, out T node)
			where T : class, ISharedNode
		{
			node = root.GetNode<T>();
		}
	}
}
