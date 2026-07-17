using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IDisabledSubscriberProxy : IProxy
	{
		public static readonly ProxyId ProxyId = 19;
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

		public delegate void OnEntityDisabledDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity callbackReceiver);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void OnEntityDisabled(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, OnEntityDisabledDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, callbackReceiver);
		}

		public delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 1);
			var __method = UnsafeExt.As<Delegate, ProxyDisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState);
		}

		public delegate Sapientia.MemoryAllocator.IndexedPtr CopyDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly Sapientia.MemoryAllocator.IndexedPtr Copy(void* __executorPtr, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 2);
			var __method = UnsafeExt.As<Delegate, CopyDelegate>(__delegate);
			return __method.Invoke(__executorPtr, oldWS, newWS, in map);
		}

		public delegate void AppendEntitiesDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void AppendEntities(void* __executorPtr, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 3);
			var __method = UnsafeExt.As<Delegate, AppendEntitiesDelegate>(__delegate);
			__method.Invoke(__executorPtr, world, ref entities);
		}

	}

	public static unsafe class IDisabledSubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void OnEntityDisabled(this in UnsafeProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			__proxyPtr.proxy.OnEntityDisabled(__proxyPtr.GetPtr().ptr, worldState, callbackReceiver);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void OnEntityDisabled(this ref ProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			__proxyPtr.proxy.OnEntityDisabled(__proxyPtr.GetPtr(__worldState).ptr, worldState, callbackReceiver);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void OnEntityDisabled(this ref ProxyEvent<IDisabledSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.OnEntityDisabled(__proxyPtr.GetPtr(__worldState).ptr, worldState, callbackReceiver);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this in UnsafeProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr().ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IDisabledSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Sapientia.MemoryAllocator.IndexedPtr Copy(this in UnsafeProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			return __proxyPtr.proxy.Copy(__proxyPtr.GetPtr().ptr, oldWS, newWS, in map);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Sapientia.MemoryAllocator.IndexedPtr Copy(this ref ProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			return __proxyPtr.proxy.Copy(__proxyPtr.GetPtr(__worldState).ptr, oldWS, newWS, in map);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Sapientia.MemoryAllocator.IndexedPtr Copy(this ref ProxyEvent<IDisabledSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState oldWS, Sapientia.MemoryAllocator.WorldState newWS, in Sapientia.MemoryAllocator.State.EntityCopyMap map)
		{
			Sapientia.MemoryAllocator.IndexedPtr __result = default;
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__result = __proxyPtr.proxy.Copy(__proxyPtr.GetPtr(__worldState).ptr, oldWS, newWS, in map);
			}
			return __result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void AppendEntities(this in UnsafeProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			__proxyPtr.proxy.AppendEntities(__proxyPtr.GetPtr().ptr, world, ref entities);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void AppendEntities(this ref ProxyPtr<IDisabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			__proxyPtr.proxy.AppendEntities(__proxyPtr.GetPtr(__worldState).ptr, world, ref entities);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void AppendEntities(this ref ProxyEvent<IDisabledSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState world, ref Sapientia.Collections.UnsafeList<Sapientia.MemoryAllocator.State.Entity> entities)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.AppendEntities(__proxyPtr.GetPtr(__worldState).ptr, world, ref entities);
			}
		}

	}

	public unsafe struct IDisabledSubscriberProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.IDisabledSubscriber
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IDisabledSubscriberProxyExt` найдите `usages` методов `OnEntityDisabled`
		private static void OnEntityDisabled(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.OnEntityDisabled(worldState, callbackReceiver);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateOnEntityDisabledDelegate()
		{
			return new IDisabledSubscriberProxy.OnEntityDisabledDelegate(OnEntityDisabled);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IDisabledSubscriberProxyExt` найдите `usages` методов `ProxyDispose`
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
			return new IDisabledSubscriberProxy.ProxyDisposeDelegate(ProxyDispose);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IDisabledSubscriberProxyExt` найдите `usages` методов `Copy`
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
			return new IDisabledSubscriberProxy.CopyDelegate(Copy);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IDisabledSubscriberProxyExt` найдите `usages` методов `AppendEntities`
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
			return new IDisabledSubscriberProxy.AppendEntitiesDelegate(AppendEntities);
		}
	}
}
