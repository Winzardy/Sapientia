using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sapientia;
using Sapientia.Pooling;
using SharedLogic.Internal;

namespace SharedLogic.Migration
{
	//TODO: wip
	// Специфичный нод который умеет изменять дату поэтому использует Manipulator
	public class MigrationSharedNode : IPersistentNode, IInitializableNode, IDisposable
	{
		[CanBeNull]
		private ILogger _logger;

		public string Id => "Migration";

		private Dictionary<string, ISharedMigrator> _idToMigrator;

		public void Initialize(ISharedRoot root)
		{
			_logger = root.Logger;
			_idToMigrator = DictionaryPool<string, ISharedMigrator>.Get();

			foreach (var node in root)
			{
				var type = node.GetType();
				// TODO: создание Migrator через рефлексию?
			}
		}

		public void Dispose()
		{
			StaticObjectPoolUtility.ReleaseAndSetNull(ref _idToMigrator);
		}

		void IPersistentNode.Load(ISharedDataManipulator manipulator)
		{
			var data = manipulator.Read<SaveData>(Id);
			OnLoad(in data, manipulator);
		}

		void IPersistentNode.Save(ISharedDataManipulator manipulator)
		{
			OnSave(out var data);
			manipulator.Write(Id, in data);
		}

		private void OnLoad(in SaveData data, ISharedDataManipulator manipulator)
		{
			foreach (var nodeData in data.nodes)
			{
				if (!_idToMigrator.TryGetValue(nodeData.nodeId, out var migrator))
					continue;

				if (nodeData.nodeVersion < migrator.Version)
				{
					// В обратную сторону не умеет мигрировать :)
					_logger?.LogWarning(
						$"Tried to migrate newer version [ {migrator.Version} ] to older [ {nodeData.nodeVersion} ] — downgrade not supported");
					continue;
				}

				if (migrator.Version != nodeData.nodeVersion)
				{
					var success = false;
					var lastVersion = 0;
					// Специально итерируем по каждой версии отдельно, чтобы плавно накатить изменения
					for (int i = migrator.Version; i < nodeData.nodeVersion; i++)
					{
						var targetVersion = i + 1;
						if (migrator.Migrate(manipulator, targetVersion))
						{
							success = true;
							lastVersion = targetVersion;
						}
					}

					if (success)
						_logger?.Log($"Node [ {nodeData.nodeId} ] migrated: {nodeData.nodeVersion} -> {lastVersion} (version)");
				}
			}
		}

		private void OnSave(out SaveData data)
		{
			using (ListPool<NodeMigrationData>.Get(out var list))
			{
				foreach (var (id, migrator) in _idToMigrator)
				{
					list.Add(new(id, migrator.Version));
				}

				data.nodes = list.ToArray();
			}
		}

		[Serializable]
		public struct SaveData
		{
			public NodeMigrationData[] nodes;
		}

		[Serializable]
		public struct NodeMigrationData
		{
			public string nodeId;
			public int nodeVersion;

			public NodeMigrationData(string nodeId, int nodeVersion)
			{
				this.nodeId = nodeId;
				this.nodeVersion = nodeVersion;
			}
		}

		public Type DataType => typeof(SaveData);
	}
}
