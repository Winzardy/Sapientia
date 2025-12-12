using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator.State
{
	public static unsafe class EntityExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExist(this Entity entity, WorldState worldState)
		{
			return worldState.GetService<EntityStatePart>().IsEntityExist(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExist(this ref Entity entity)
		{
			if (!entity.worldId.IsValid())
				return false;
			return !entity.IsEmpty() && entity.IsExist(entity.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return entity.Has<T>(entity.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this Entity entity, WorldState worldState) where T: unmanaged, IComponent
		{
			return worldState.GetComponentSet<T>().HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRead<T>(this ref Entity entity, out T result) where T: unmanaged, IComponent
		{
			return entity.TryRead<T>(entity.GetWorldState(), out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRead<T>(this ref Entity entity, WorldState worldState, out T result) where T: unmanaged, IComponent
		{
			return worldState.GetComponentSet<T>().TryReadElement<T>(worldState, entity, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return ref entity.Read<T>(entity.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this Entity entity, WorldState worldState) where T: unmanaged, IComponent
		{
			return ref worldState.GetComponentSet<T>().ReadElement<T>(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this Entity entity, WorldState worldState, out bool isExist) where T: unmanaged, IComponent
		{
			return ref worldState.GetComponentSet<T>().ReadElement<T>(worldState, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return ref entity.Get<T>(entity.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this Entity entity, WorldState worldState) where T: unmanaged, IComponent
		{
			return ref worldState.GetComponentSet<T>().GetElement<T>(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this ref Entity entity, out bool isCreated) where T: unmanaged, IComponent
		{
			return ref entity.Get<T>(entity.GetWorldState(), out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this Entity entity, WorldState worldState, out bool isCreated) where T: unmanaged, IComponent
		{
			return ref worldState.GetComponentSet<T>().GetElement<T>(worldState, entity, out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Remove<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return entity.Remove<T>(entity.GetWorldState());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Remove<T>(this ref Entity entity, WorldState worldState) where T: unmanaged, IComponent
		{
			return worldState.GetComponentSet<T>().RemoveSwapBackElement(worldState, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ComponentSetContext<T> GetArchetype<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return new ComponentSetContext<T>(entity.worldId.GetWorldState());
		}
	}
}
