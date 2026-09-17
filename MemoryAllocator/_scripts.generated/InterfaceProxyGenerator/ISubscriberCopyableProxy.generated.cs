using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe struct ISubscriberCopyableProxy : IProxy
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

		public delegate Sapientia.MemoryAllocator.IndexedPtr CopyDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly Sapientia.MemoryAllocator.IndexedPtr Copy(void* __executorPtr, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, CopyDelegate>(__delegate);
			return __method.Invoke(__executorPtr, oldWS, newWS, in map);
		}

		public delegate void AppendEntitiesDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void AppendEntities(void* __executorPtr, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 1);
			var __method = UnsafeExt.As<Delegate, AppendEntitiesDelegate>(__delegate);
			__method.Invoke(__executorPtr, world, ref entities);
		}

		public delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 2);
			var __method = UnsafeExt.As<Delegate, ProxyDisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState);
		}

	}

	public static unsafe class ISubscriberCopyableProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Sapientia.MemoryAllocator.IndexedPtr Copy(this in UnsafeProxyPtr<ISubscriberCopyableProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			return __proxyPtr.proxy.Copy(__proxyPtr.GetPtr().ptr, oldWS, newWS, in map);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Sapientia.MemoryAllocator.IndexedPtr Copy(this ref ProxyPtr<ISubscriberCopyableProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			return __proxyPtr.proxy.Copy(__proxyPtr.GetPtr(__worldState).ptr, oldWS, newWS, in map);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Sapientia.MemoryAllocator.IndexedPtr Copy(this ref ProxyEvent<ISubscriberCopyableProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			Sapientia.MemoryAllocator.IndexedPtr __result = default;
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__result = __proxyPtr.proxy.Copy(__proxyPtr.GetPtr(__worldState).ptr, oldWS, newWS, in map);
			}
			return __result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void AppendEntities(this in UnsafeProxyPtr<ISubscriberCopyableProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			__proxyPtr.proxy.AppendEntities(__proxyPtr.GetPtr().ptr, world, ref entities);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void AppendEntities(this ref ProxyPtr<ISubscriberCopyableProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			__proxyPtr.proxy.AppendEntities(__proxyPtr.GetPtr(__worldState).ptr, world, ref entities);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void AppendEntities(this ref ProxyEvent<ISubscriberCopyableProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.AppendEntities(__proxyPtr.GetPtr(__worldState).ptr, world, ref entities);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this in UnsafeProxyPtr<ISubscriberCopyableProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr().ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<ISubscriberCopyableProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<ISubscriberCopyableProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
			}
		}

	}

	public unsafe struct ISubscriberCopyableProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.ISubscriberCopyable
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `ISubscriberCopyableProxyExt` найдите `usages` методов `Copy`
		private static Sapientia.MemoryAllocator.IndexedPtr Copy(void* executorPtr, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
return default;
#else
			return __source.Copy(oldWS, newWS, in map);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateCopyDelegate()
		{
			return new ISubscriberCopyableProxy.CopyDelegate(Copy);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `ISubscriberCopyableProxyExt` найдите `usages` методов `AppendEntities`
		private static void AppendEntities(void* executorPtr, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.AppendEntities(world, ref entities);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateAppendEntitiesDelegate()
		{
			return new ISubscriberCopyableProxy.AppendEntitiesDelegate(AppendEntities);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `ISubscriberCopyableProxyExt` найдите `usages` методов `ProxyDispose`
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
			return new ISubscriberCopyableProxy.ProxyDisposeDelegate(ProxyDispose);
		}
	}
}
