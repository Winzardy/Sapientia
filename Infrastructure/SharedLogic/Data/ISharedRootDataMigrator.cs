using Sapientia;
using System;
using System.Collections.Generic;

namespace SharedLogic
{
	public interface ISharedRootDataMigrator<TRawData>
	{
		void Begin(in TRawData data);
		void Finalize(in TRawData data);

		void MigrateData<T>(in T data, ISharedMigrationsRegistry<T> migrations);
	}

	public interface ISharedLogicMigration<TData>
	{
		void Migrate(in TData data, ILogger? logger);
	}

	public interface ISharedMigrationsRegistry<TData>
	{
		IEnumerable<ISharedLogicMigration<TData>> GetMigrations(int version);
	}

	public abstract class SharedDataMigrator<TData> : ISharedRootDataMigrator<TData>
	{
		private readonly ILogger? _logger;
		private int _savedVersion;

		private bool _began;
		private readonly string _typeName;

		public abstract int CurrentVersion { get; }

		protected SharedDataMigrator(ILogger? logger)
		{
			_logger = logger;
			_typeName = GetType().Name;
		}

		public virtual void Begin(in TData data)
		{
			if (_began)
				throw new InvalidOperationException("Migrations already began.");

			_savedVersion = GetCurrentVersion(data);

			if (_savedVersion == CurrentVersion)
				return;

			_began = true;
			_logger?.Log($"[ {_typeName} ]: Start migrating data [ {typeof(TData).Name} ]");
		}

		public virtual void Finalize(in TData data)
		{
			if (_savedVersion == CurrentVersion)
				return;

			if (!_began)
				throw new InvalidOperationException("Migrations did not start.");

			SetCurrentVersion(data);
			_began = false;

			_logger?.Log($"[ {_typeName} ]: Complete migrating data [ {typeof(TData).Name} ]");
		}

		protected abstract int GetCurrentVersion(in TData data);
		protected abstract void SetCurrentVersion(in TData data);

		public bool IsMigrationNeeded(in TData data) => GetCurrentVersion(data) != CurrentVersion;

		public void MigrateData<T>(in T data, ISharedMigrationsRegistry<T> migrations)
		{
			if (_savedVersion == CurrentVersion)
				return;

			if (!_began)
				throw new InvalidOperationException("Migrations did not start.");

			foreach (var migration in migrations.GetMigrations(_savedVersion))
			{
				var migrationName = migration.GetType().Name;

				try
				{
					_logger?.Log($"[ {_typeName} ]: Performing {migrationName}...");
					migration.Migrate(data, _logger);
					_logger?.Log($"[ {_typeName} ]: {migrationName} Complete.");
				}
				catch (Exception e)
				{
					_logger?.LogError($"[ {_typeName} ]: Migration {migrationName} failed.");
					_logger?.LogException(e);
				}
			}
		}
	}
}
