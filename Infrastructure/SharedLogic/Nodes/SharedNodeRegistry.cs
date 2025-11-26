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

		// Важен порядок, потому не использую один Dictionary
		private List<ISharedNode> _nodes;
		private Dictionary<string, ISharedNode> _idToNode;

		private bool _initialized;

		private readonly int _index;

		internal SharedNodeRegistry()
		{
			_index = Interlocked.Increment(ref _i) - 1;

			_nodes = ListPool<ISharedNode>.Get();
			_idToNode = DictionaryPool<string, ISharedNode>.Get();
		}

		public void Dispose()
		{
			Disposed?.Invoke(_index);

			StaticObjectPoolUtility.ReleaseAndSetNull(ref _nodes);
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _idToNode);
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

		internal ISharedNode GetNode(string id) => _idToNode[id];

		internal bool TryGetNode(string id, out ISharedNode node) => _idToNode.TryGetValue(id, out node);

		internal IEnumerator<ISharedNode> GetEnumerator() => _nodes.GetEnumerator();

		public void Register<T>(T node) where T : ISharedNode
		{
			_nodes.Add(node);
			_idToNode.Add(node.Id, node);

			SharedNodeRegistry<T>.Register(_index, node);
		}

		public void Register<T>() where T : ISharedNode, new()
			=> Register(new T());
	}

	internal static class SharedNodeRegistry<T>
		where T : ISharedNode
	{
		private static ConcurrentDictionary<int, T> _indexToNode;
		private static readonly object _gate = new();

		static SharedNodeRegistry()
		{
			SharedNodeRegistry.Disposed += OnRegistryDisposed;
		}

		private static void OnRegistryDisposed(int index)
		{
			var map = Volatile.Read(ref _indexToNode);
			if (map == null)
				return;

			map.TryRemove(index, out _);
		}

		public static void Register(int index, T node)
		{
			EnsureMap().TryAdd(index, node);
		}

		private static ConcurrentDictionary<int, T> EnsureMap()
		{
			var map = Volatile.Read(ref _indexToNode);
			if (map != null)
				return map;

			lock (_gate)
			{
				if (_indexToNode == null)
					_indexToNode = new();
				return _indexToNode;
			}
		}

		public static bool TryGet(int index, out T node)
		{
			var map = Volatile.Read(ref _indexToNode);
			if (map == null)
			{
				node = default;
				return false;
			}

			return map.TryGetValue(index, out node);
		}
	}
}
