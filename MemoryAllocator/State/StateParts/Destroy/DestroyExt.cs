namespace Sapientia.MemoryAllocator.State
{
	/*public static unsafe class DestroyExt
	{
		public static void RequestDestroy(this Entity entity, SafePtr<Allocator> allocator)
		{
			Debug.Assert(entity.IsAlive(allocator));

			entity.Get<DestroyRequest>(allocator);

			Debug.Assert(!entity.Has<KillElement>(allocator));
		}

		public static void RequestDestroy(this ref Entity entity)
		{
			entity.RequestDestroy(entity.GetAllocatorPtr());
		}

		public static bool HasDestroyRequest(this Entity entity, SafePtr<Allocator> allocator)
		{
			return entity.Has<DestroyRequest>(allocator);
		}

		public static bool HasDestroyRequest(this ref Entity entity)
		{
			return entity.HasDestroyRequest(entity.GetAllocatorPtr());
		}

		public static void RequestKill(this Entity entity, SafePtr<Allocator> allocator)
		{
			Debug.Assert(entity.IsAlive());
			entity.Get<KillRequest>(allocator);
		}

		public static void RequestKill(this ref Entity entity)
		{
			entity.RequestKill(entity.GetAllocatorPtr());
		}

		public static void RequestKill(this Entity entity, SafePtr<Allocator> allocator, float delay)
		{
			Debug.Assert(entity.IsAlive(allocator));
			if (entity.Has<KillRequest>(allocator))
				return;
			ref var request = ref entity.Get<DelayKillRequest>(allocator, out var hasRequest);
			if (!hasRequest || request.delay > delay)
				request.delay = delay;
		}

		public static void RequestKill(this ref Entity entity, float delay)
		{
			entity.RequestKill(entity.GetAllocatorPtr(), delay);
		}

		public static bool HasKillRequest(this Entity entity, SafePtr<Allocator> allocator)
		{
			return entity.Has<KillRequest>(allocator);
		}

		public static bool HasKillRequest(this ref Entity entity)
		{
			return entity.HasKillRequest(entity.GetAllocatorPtr());
		}

		public static bool HasDestroyOrKillRequest(this Entity entity, SafePtr<Allocator> allocator)
		{
			return entity.Has<KillRequest>(allocator) || entity.Has<DestroyRequest>(allocator);
		}

		public static bool HasDestroyOrKillRequest(this ref Entity entity)
		{
			return entity.HasDestroyOrKillRequest(entity.GetAllocatorPtr());
		}

		public static void AddKillParent(this Entity child, SafePtr<Allocator> allocator, Entity parent)
		{
			Debug.Assert(child.IsAlive(allocator));
			Debug.Assert(parent.IsAlive(allocator));

			ref var childElement = ref child.Get<KillElement>(allocator);
			ref var parentElement = ref parent.Get<KillElement>(allocator);

			if (!childElement.parents.IsCreated)
				childElement.parents = new List<Entity>(allocator);
			if (!childElement.children.IsCreated)
				childElement.children = new List<Entity>(allocator);

			childElement.parents.Add(allocator, parent);
			parentElement.children.Add(allocator, child);
		}

		public static void AddKillParent(this ref Entity child, Entity parent)
		{
			child.AddKillParent(child.GetAllocatorPtr(), parent);
		}

		public static void AddKillChild(this ref Entity parent, SafePtr<Allocator> allocator, Entity child)
		{
			child.AddKillParent(allocator, parent);
		}

		public static void AddKillChild(this ref Entity parent, Entity child)
		{
			child.AddKillParent(parent.GetAllocatorPtr(), parent);
		}

		public static void AddKillChildren<T>(this Entity parent, SafePtr<Allocator> allocator, in T children) where T: IEnumerable<Entity>
		{
			Debug.Assert(parent.IsAlive(allocator));

			ref var parentElement = ref parent.Get<KillElement>(allocator);

			if (!parentElement.children.IsCreated)
				parentElement.children = new List<Entity>(allocator);
			parentElement.children.AddRange(allocator, children);

			foreach (var entity in children)
			{
				var child = entity;

				Debug.Assert(child.IsAlive(allocator));
				ref var childElement = ref child.Get<KillElement>(allocator);
				if (!childElement.parents.IsCreated)
					childElement.parents = new List<Entity>(allocator);
				childElement.parents.Add(allocator, parent);
			}
		}

		public static void AddKillChildren<T>(this ref Entity parent, in T children) where T: IEnumerable<Entity>
		{
			parent.AddKillChildren(parent.GetAllocatorPtr(), children);
		}

		public static void AddKillCallback<T>(this Entity holder, SafePtr<Allocator> allocator, Entity target, in T callback = default) where T: unmanaged, IKillSubscriber
		{
			ref var holderElement = ref holder.Get<KillElement>(allocator);

			if (!holderElement.killCallbacks.IsCreated)
				holderElement.killCallbacks = new List<KillCallback>(allocator);
			holderElement.killCallbacks.Add(allocator, new KillCallback
			{
				callback = ProxyPtr<IKillSubscriberProxy>.Create(allocator, callback),
				target = target,
			});

			ref var targetElement = ref target.Get<KillElement>(allocator);
			if (!targetElement.killCallbackHolders.IsCreated)
				targetElement.killCallbackHolders = new List<Entity>(allocator);
			targetElement.killCallbackHolders.Add(allocator, holder);
		}

		public static void AddKillCallback<T>(this Entity holder, Entity target, in T callback = default) where T: unmanaged, IKillSubscriber
		{
			holder.AddKillCallback(holder.GetAllocatorPtr(), target, callback);
		}

		public static bool IsAlive(this Entity entity, SafePtr<Allocator> allocator)
		{
			return !entity.Has<DestroyRequest>(allocator) &&
			       !entity.Has<KillRequest>(allocator) &&
			       entity.IsExist(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsAlive(this ref Entity entity)
		{
			return IsAlive(entity, entity.GetAllocatorPtr());
		}

		public static bool IsDead(this Entity entity, SafePtr<Allocator> allocator)
		{
			return entity.Has<DestroyRequest>(allocator) ||
			       entity.Has<KillRequest>(allocator) ||
			       !entity.IsExist(allocator);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsDead(this ref Entity entity)
		{
			return IsDead(entity, entity.GetAllocatorPtr());
		}
	}*/
}
