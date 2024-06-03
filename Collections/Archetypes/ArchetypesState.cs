using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Sapientia.Extensions;

namespace Sapientia.Collections.Archetypes
{
	public static class ArchetypesStateExt
	{
		public static Archetype<T> RegisterArchetype<T>(this WorldStatePart statePart, SparseSet<ArchetypeElement<T>>.ResetAction resetAction = null,
			Archetype<T>.DestroyEvents? destroyEvents = null)
		{
			return ServiceLocator<ArchetypesState>.Instance.RegisterArchetype(resetAction, destroyEvents);
		}

		public static Archetype<T> RegisterArchetype<T>(this WorldStatePart statePart, int elementsCount, SparseSet<ArchetypeElement<T>>.ResetAction resetAction = null,
			Archetype<T>.DestroyEvents? destroyEvents = null)
		{
			return ServiceLocator<ArchetypesState>.Instance.RegisterArchetype(elementsCount, resetAction, destroyEvents);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool Has<T>(this Entity entity)
		{
			return ServiceLocator<Archetype<T>>.Instance.HasElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref readonly T Read<T>(this Entity entity)
		{
			return ref ServiceLocator<Archetype<T>>.Instance.ReadElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ref T Get<T>(this Entity entity)
		{
			return ref ServiceLocator<Archetype<T>>.Instance.GetElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static void Remove<T>(this Entity entity)
		{
			ServiceLocator<Archetype<T>>.Instance.RemoveSwapBackElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype<T> GetArchetype<T>(this Entity entity)
		{
			return ServiceLocator<Archetype<T>>.Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype<T> GetArchetype<T>(this WorldSystem system)
		{
			return ServiceLocator<Archetype<T>>.Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype<T> GetArchetype<T>(this WorldStatePart statePart)
		{
			return ServiceLocator<Archetype<T>>.Instance;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Archetype<T> GetArchetype<T>()
		{
			return ServiceLocator<Archetype<T>>.Instance;
		}
	}

	public class ArchetypesState : WorldStatePart
	{
		private readonly SimpleList<BaseArchetype> _archetypes = new();

		public Archetype<T> RegisterArchetype<T>(SparseSet<ArchetypeElement<T>>.ResetAction resetAction = null,
			Archetype<T>.DestroyEvents? destroyEvents = null)
		{
			return RegisterArchetype(ServiceLocator<EntitiesState>.Instance.EntitiesCapacity, resetAction, destroyEvents);
		}

		public Archetype<T> RegisterArchetype<T>(int elementsCount, SparseSet<ArchetypeElement<T>>.ResetAction resetAction = null,
			Archetype<T>.DestroyEvents? destroyEvents = null)
		{
			return RegisterArchetype(elementsCount, ServiceLocator<EntitiesState>.Instance.EntitiesCapacity, resetAction, destroyEvents);
		}

		private Archetype<T> RegisterArchetype<T>(int elementsCount, int entitiesCapacity, SparseSet<ArchetypeElement<T>>.ResetAction resetAction = null, Archetype<T>.DestroyEvents? destroyEvents = null)
		{
			var archetype = new Archetype<T>(elementsCount, entitiesCapacity, resetAction, destroyEvents);
			ServiceLocator<Archetype<T>>.ReplaceService(archetype);

			_archetypes.Add(archetype);
			return archetype;
		}
	}
}