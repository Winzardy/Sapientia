using System.Runtime.CompilerServices;
using Sapientia.Data;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator.State
{
	public partial struct DestroyLogic
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RequestDestroy(Entity entity)
		{
			E.ASSERT(IsAlive(entity));

			_destroyRequestSet.GetElement(entity);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool HasDestroyRequest(Entity entity)
		{
			return _destroyRequestSet.HasElement(entity);
		}

		public void AddDestroyParent(Entity child, Entity parent)
		{
			E.ASSERT(IsAlive(child));
			E.ASSERT(_entityStatePart.Value().IsEntityExist(_worldState, parent));

			if (_destroyRequestSet.HasElement(parent))
			{
				RequestKill(child);
				return;
			}

			ref var childElement = ref _destroyElementSet.GetElement(child);
			ref var parentElement = ref _destroyElementSet.GetElement(parent);

			if (!childElement.parents.IsCreated)
				childElement.parents = new MemList<Entity>(_worldState);
			if (!childElement.children.IsCreated)
				childElement.children = new MemList<Entity>(_worldState);

			childElement.parents.Add(_worldState, parent);
			parentElement.children.Add(_worldState, child);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void AddDestroyChild(Entity parent, Entity child)
		{
			AddDestroyParent(child, parent);
		}

		public void AddDestroyChildren(Entity parent, MemListEnumerable<Entity> children)
		{
			E.ASSERT(IsAlive(parent));

			ref var parentElement = ref _destroyElementSet.GetElement(parent);

			if (!parentElement.children.IsCreated)
				parentElement.children = new MemList<Entity>(_worldState);
			parentElement.children.AddRange(_worldState, children);

			foreach (var child in children)
			{
				E.ASSERT(IsAlive(child));
				ref var childElement = ref _destroyElementSet.GetElement(child);
				if (!childElement.parents.IsCreated)
					childElement.parents = new MemList<Entity>(_worldState);
				childElement.parents.Add(_worldState, parent);
			}
		}
	}
}
