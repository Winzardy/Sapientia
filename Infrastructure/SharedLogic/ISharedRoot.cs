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

		T GetNode<T>() where T : class, ISharedNode;
		ISharedNode GetNode(string id);
		bool TryGetNode(string id, out ISharedNode node);

		void Initialize();
		void Save(ISharedDataStreamer streamer);
		void Load(ISharedDataStreamer streamer);

		IEnumerable<TFilter> Enumerate<TFilter>();

		int Revision { get; }

		void OnExecuted<T>(in T command)
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
