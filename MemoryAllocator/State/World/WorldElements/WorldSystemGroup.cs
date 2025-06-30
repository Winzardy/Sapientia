namespace Sapientia.MemoryAllocator
{
	public abstract class WorldSystemGroup
	{
		private WorldBuilder _builder;

		public void AddSystems(WorldBuilder builder)
		{
			_builder = builder;
			AddSystems();
		}

		protected abstract void AddSystems();

		protected void AddSystemGroup<T>() where T: WorldSystemGroup, new()
		{
			_builder.AddSystemGroup<T>();
		}

		protected void AddSystem<T>() where T: unmanaged, IWorldSystem
		{
			_builder.AddSystem<T>();
		}
	}
}
