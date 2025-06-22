using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator.State
{
	public static unsafe class EntityExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExist(this Entity entity, World world)
		{
			return world.GetService<EntityStatePart>().IsEntityExist(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExist(this ref Entity entity)
		{
			if (!entity.worldId.IsValid())
				return false;
			return !entity.IsEmpty() && entity.IsExist(entity.GetWorld());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return entity.Has<T>(entity.GetWorld());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this Entity entity, World world) where T: unmanaged, IComponent
		{
			return world.GetArchetype<T>().HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRead<T>(this ref Entity entity, out T result) where T: unmanaged, IComponent
		{
			return entity.TryRead<T>(entity.GetWorld(), out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRead<T>(this ref Entity entity, World world, out T result) where T: unmanaged, IComponent
		{
			return world.GetArchetype<T>().TryReadElement<T>(world, entity, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return ref entity.Read<T>(entity.GetWorld());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this Entity entity, World world) where T: unmanaged, IComponent
		{
			return ref world.GetArchetype<T>().ReadElement<T>(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this Entity entity, World world, out bool isExist) where T: unmanaged, IComponent
		{
			return ref world.GetArchetype<T>().ReadElement<T>(world, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return ref entity.Get<T>(entity.GetWorld());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this Entity entity, World world) where T: unmanaged, IComponent
		{
			return ref world.GetArchetype<T>().GetElement<T>(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this ref Entity entity, out bool isCreated) where T: unmanaged, IComponent
		{
			return ref entity.Get<T>(entity.GetWorld(), out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this Entity entity, World world, out bool isCreated) where T: unmanaged, IComponent
		{
			return ref world.GetArchetype<T>().GetElement<T>(world, entity, out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			entity.Remove<T>(entity.GetWorld());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(this ref Entity entity, World world) where T: unmanaged, IComponent
		{
			world.GetArchetype<T>().RemoveSwapBackElement(world, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArchetypeContext<T> GetArchetype<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return new ArchetypeContext<T>(entity.worldId.GetWorld());
		}
	}
}
