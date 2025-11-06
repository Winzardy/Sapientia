using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IEntityDestroySubscriberProxy : IProxy
	{
		public static readonly ProxyId ProxyId = 18;
		ProxyId IProxy.ProxyId
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => ProxyId;
		}

		private DelegateIndex _firstDelegateIndex;
		DelegateIndex IProxy.FirstDelegateIndex
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => _firstDelegateIndex;
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			set => _firstDelegateIndex = value;
		}

		public delegate void EntityArrayDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity* entities, System.Int32 count);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityArrayDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity* entities, System.Int32 count)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, EntityArrayDestroyedDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, entities, count);
		}

		public delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 1);
			var __method = UnsafeExt.As<Delegate, ProxyDisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState);
		}

	}

	public static unsafe class IEntityDestroySubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this in UnsafeProxyPtr<IEntityDestroySubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity* entities, System.Int32 count)
		{
			__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr().ptr, worldState, entities, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyPtr<IEntityDestroySubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity* entities, System.Int32 count)
		{
			__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr(__worldState).ptr, worldState, entities, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyEvent<IEntityDestroySubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity* entities, System.Int32 count)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr(__worldState).ptr, worldState, entities, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this in UnsafeProxyPtr<IEntityDestroySubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr().ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IEntityDestroySubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IEntityDestroySubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
			}
		}

	}

	public unsafe struct IEntityDestroySubscriberProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.IEntityDestroySubscriber
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IEntityDestroySubscriberProxyExt` найдите `usages` методов `EntityArrayDestroyed`
		private static void EntityArrayDestroyed(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity* entities, System.Int32 count)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityArrayDestroyed(worldState, entities, count);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateEntityArrayDestroyedDelegate()
		{
			return new IEntityDestroySubscriberProxy.EntityArrayDestroyedDelegate(EntityArrayDestroyed);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IEntityDestroySubscriberProxyExt` найдите `usages` методов `ProxyDispose`
		private static void ProxyDispose(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.ProxyDispose(worldState);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateProxyDisposeDelegate()
		{
			return new IEntityDestroySubscriberProxy.ProxyDisposeDelegate(ProxyDispose);
		}
	}
}
