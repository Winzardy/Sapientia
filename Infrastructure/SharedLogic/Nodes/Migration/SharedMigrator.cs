namespace SharedLogic.Migration
{
	internal interface ISharedMigrator
	{
		// TODO: переделать в static, когда завезут C# 11
		/// <summary>
		/// Статичная версия в коде, сохранять не надо! назначаем хардкодом!
		/// </summary>
		public int Version { get; }

		/// <param name="targetVersion">Версия которую пытаемся накатить</param>
		public bool Migrate(ISharedDataManipulator manipulator, int targetVersion);
	}

	public abstract class SharedMigrator<TData> : ISharedMigrator
	{
		private readonly string _nodeId;

		public abstract int Version { get; }

		public SharedMigrator(string nodeId)
		{
			_nodeId = nodeId;
		}

		public bool Migrate(ISharedDataManipulator manipulator, int targetVersion)
		{
			var data = manipulator.Read<TData>(_nodeId);
			if (OnMigrate(ref data, targetVersion))
			{
				manipulator.Write(_nodeId, data);
				return true;
			}

			return false;
		}

		protected abstract bool OnMigrate(ref TData data, int targetVersion);
	}
}
