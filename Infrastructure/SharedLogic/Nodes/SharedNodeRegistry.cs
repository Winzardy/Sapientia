using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Sapientia.Collections;
using Sapientia.Pooling;
using Sapientia.Pooling.Concurrent;

namespace SharedLogic
{
	/// <summary>
	/// Отвечает за регистрацию набора нодов в реестр, определяя их состав и порядок инициализации
	/// </summary>
	public interface ISharedNodesRegistrar
	{
		public void Register(ISharedNodeRegistry registry);
	}

	/// <summary>
	/// Реестр нодов
	/// </summary>
	public interface ISharedNodeRegistry
	{
		public void Register<T>(T node) where T : ISharedNode;
		public void Register<T>() where T : ISharedNode, new();
	}

	internal class SharedNodeRegistry : IDisposable, ISharedNodeRegistry
	{
		internal static event Action<int> Disposed;

		private static int _i;

		private List<ISharedNode> _nodes;

		private bool _initialized;

		private readonly int _index;

		internal SharedNodeRegistry()
		{
			_index = Interlocked.Increment(ref _i) - 1;
			_nodes = ListPool<ISharedNode>.Get();
		}

		public void Dispose()
		{
			Disposed?.Invoke(_index);

			StaticObjectPoolUtility.ReleaseAndSetNull(ref _nodes);
		}

		internal IEnumerable<T> FilterBy<T>()
		{
			if (_nodes.IsNullOrEmpty())
				yield break;

			foreach (var node in _nodes)
			{
				if (node is T filteredNode)
					yield return filteredNode;
			}
		}

		internal T GetNode<T>() where T : ISharedNode
		{
			if (SharedNodeRegistry<T>.TryGet(_index, out var node))
				return node;

			throw new Exception($"Not found node by type [ {typeof(T)} ] in registry by index [ {_index} ]");
		}

		public void Register<T>(T node) where T : ISharedNode
		{
			_nodes.Add(node);
			SharedNodeRegistry<T>.Register(_index, node);
		}

		public void Register<T>() where T : ISharedNode, new()
			=> Register(new T());
	}

	internal static class SharedNodeRegistry<T>
		where T : ISharedNode
	{
		private static ConcurrentDictionary<int, T> _indexToNode;

		static SharedNodeRegistry()
		{
			SharedNodeRegistry.Disposed += OnRegistryDisposed;
		}

		private static void OnRegistryDisposed(int index)
		{
			if (_indexToNode == null)
				return;

			if (!_indexToNode.ContainsKey(index))
				return;

			if (!_indexToNode.TryRemove(index, out _))
				return;

			if (_indexToNode.IsEmpty)
				StaticObjectPoolUtility.ReleaseAndSetNull(ref _indexToNode);
		}

		public static void Register(int index, T node)
		{
			_indexToNode ??= ConcurrentDictionaryPool<int, T>.Get();
			_indexToNode.TryAdd(index, node);
		}

		public static bool TryGet(int index, out T node)
		{
			node = default;
			if (_indexToNode == null)
				return false;

			return _indexToNode.TryGetValue(index, out node);
		}
	}
}
