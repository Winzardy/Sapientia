using Sapientia.MemoryAllocator.State;

namespace Sapientia.MemoryAllocator
{
	public abstract class WorldStatePartGroup
	{
		private WorldBuilder _builder;

		public void AddStateParts(WorldBuilder builder)
		{
			_builder = builder;
			AddStateParts();
		}

		protected abstract void AddStateParts();

		protected void AddStatePartGroup<T>() where T: WorldSystemGroup, new()
		{
			_builder.AddSystemGroup<T>();
		}

		protected void AddStatePart<T>(in T value = default) where T: unmanaged, IWorldStatePart
		{
			_builder.AddStatePart<T>(value);
		}

		protected void AddUnmanagedLocalStatePart<T>() where T: unmanaged, IWorldUnmanagedLocalStatePart
		{
			_builder.AddUnmanagedLocalStatePart<T>();
		}

		protected void AddLocalStatePart<T>() where T: class, IWorldLocalStatePart, new()
		{
			_builder.AddLocalStatePart<T>();
		}

		protected void AddLocalStatePart<T>(in T value) where T: class, IWorldLocalStatePart
		{
			_builder.AddLocalStatePart<T>(value);
		}
	}
}
