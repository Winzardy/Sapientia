using System.Runtime.CompilerServices;

namespace Sapientia.MemoryAllocator.State
{
	public static unsafe class EntityExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExist(this Entity entity, Allocator* allocator)
		{
			return allocator->GetService<EntityStatePart>().IsEntityExist(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsExist(this ref Entity entity)
		{
			if (!entity.allocatorId.IsValid())
				return false;
			return !entity.IsEmpty() && entity.IsExist(entity.GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return entity.Has<T>(entity.GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this Entity entity, Allocator* allocator) where T: unmanaged, IComponent
		{
			return allocator->GetArchetype<T>().HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRead<T>(this ref Entity entity, out T result) where T: unmanaged, IComponent
		{
			return entity.TryRead<T>(entity.GetAllocatorPtr(), out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryRead<T>(this ref Entity entity, Allocator* allocator, out T result) where T: unmanaged, IComponent
		{
			return allocator->GetArchetype<T>().TryReadElement<T>(allocator, entity, out result);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return ref entity.Read<T>(entity.GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this Entity entity, Allocator* allocator) where T: unmanaged, IComponent
		{
			return ref allocator->GetArchetype<T>().ReadElement<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this Entity entity, Allocator* allocator, out bool isExist) where T: unmanaged, IComponent
		{
			return ref allocator->GetArchetype<T>().ReadElement<T>(allocator, entity, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return ref entity.Get<T>(entity.GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this Entity entity, Allocator* allocator) where T: unmanaged, IComponent
		{
			return ref allocator->GetArchetype<T>().GetElement<T>(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this ref Entity entity, out bool isCreated) where T: unmanaged, IComponent
		{
			return ref entity.Get<T>(entity.GetAllocatorPtr(), out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this Entity entity, Allocator* allocator, out bool isCreated) where T: unmanaged, IComponent
		{
			return ref allocator->GetArchetype<T>().GetElement<T>(allocator, entity, out isCreated);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			entity.Remove<T>(entity.GetAllocatorPtr());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(this ref Entity entity, Allocator* allocator) where T: unmanaged, IComponent
		{
			allocator->GetArchetype<T>().RemoveSwapBackElement(allocator, entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ArchetypeContext<T> GetArchetype<T>(this ref Entity entity) where T: unmanaged, IComponent
		{
			return new ArchetypeContext<T>(entity.allocatorId.GetAllocatorPtr());
		}
	}
}
