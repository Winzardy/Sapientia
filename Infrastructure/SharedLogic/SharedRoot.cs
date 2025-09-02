using System;
using Sapientia;
using Sapientia.Extensions;

namespace SharedLogic
{
	public class SharedRoot : ISharedRoot
	{
		private bool _initialized;

		private readonly ISharedNodesRegistrar _registrar;
		private readonly ILogger _logger;

		private SharedNodeRegistry _registry;

		ILogger ISharedRoot.Logger => _logger;

		public SharedRoot(ISharedNodesRegistrar registrar, ILogger logger = null)
		{
			_registrar = registrar;
			_logger = logger;

			_registry = new SharedNodeRegistry();

			// Системные ноды
			_registry.Register<TimeSharedNode>();

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

		public void Load(ISharedDataReader reader)
		{
			foreach (var node in _registry.FilterBy<IPersistentNode>())
				node.Load(reader);
		}

		public void Save(ISharedDataWriter writer)
		{
			foreach (var node in _registry.FilterBy<IPersistentNode>())
				node.Save(writer);
		}

		public T GetNode<T>() where T : class, ISharedNode
			=> _registry.GetNode<T>();
	}
}
