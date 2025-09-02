#nullable enable
using System;
using Sapientia;

namespace SharedLogic
{
	public interface ISharedRoot : IDisposable
	{
		ILogger Logger { get; }
		public T GetNode<T>() where T : class, ISharedNode;

		public void Initialize();
		public void Save(ISharedDataWriter writer);
		public void Load(ISharedDataReader reader);
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
