using System;
using System.Collections;
using System.Collections.Generic;
using Sapientia;
using Sapientia.Extensions;
using SharedLogic.Internal;
using SharedLogic.Migration;

namespace SharedLogic
{
	public class SharedRoot : ISharedRoot
	{
		private const string REVISION_KEY = "Revision";

		private bool _initialized;
		private int _revision;

		private readonly ISharedNodesRegistrar _registrar;
		private readonly ILogger _logger;

		private SharedNodeRegistry _registry;

		ILogger ISharedRoot.Logger => _logger;

		public event Action Loaded;
		public event Action Saved;

		public int Revision => _revision;

		public SharedRoot(ISharedNodesRegistrar registrar, IDateTimeProvider dateTimeProvider, ILogger logger = null)
		{
			_registrar = registrar;
			_logger = logger;

			_registry = new SharedNodeRegistry();

			// Системные ноды
			_registry.Register(new MigrationSharedNode());
			_registry.Register(new TimeSharedNode(dateTimeProvider));

			_registrar.Register(_registry);
		}

		public void Dispose()
		{
			foreach (var node in _registry.FilterBy<IDisposable>())
				node.Dispose();

			DisposeUtility.DisposeAndSetNull(ref _registry);
		}

		public void Initialize()
		{
			if (_initialized)
				return;

			_initialized = true;

			foreach (var node in _registry.FilterBy<IInitializableNode>())
				node.Initialize(this);
		}

		public T GetNode<T>() where T : class, ISharedNode
			=> _registry.GetNode<T>();

		public IEnumerable<TFilter> Enumerate<TFilter>()
		{
			foreach (var node in _registry.FilterBy<TFilter>())
				yield return node;
		}

		public ISharedNode GetNode(string id) => _registry.GetNode(id);
		public bool TryGetNode(string id, out ISharedNode node) => _registry.TryGetNode(id, out node);

		public IEnumerator<ISharedNode> GetEnumerator() => _registry.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public void OnExecuted<T>(in T command)
			where T : struct, ICommand
		{
			if (SharedTimeUtility.IsTimedCommand(in command))
				return;

			_revision++;

			if (SLDebug.Logging.Command.execute)
				_logger?.Log($"Executed command by type [ {command.GetType().Name} ] (rev: {_revision})");
		}

		public void Load(ISharedDataStreamer streamer)
		{
			foreach (var node in _registry.FilterBy<IPersistentNode>())
				node.Load(streamer);

			_revision = streamer.Read<int>(REVISION_KEY);

			Loaded?.Invoke();
		}

		public void Save(ISharedDataStreamer streamer)
		{
			foreach (var node in _registry.FilterBy<IPersistentNode>())
				node.Save(streamer);

			streamer.Write(REVISION_KEY, _revision);

			Saved?.Invoke();
		}
	}
}
