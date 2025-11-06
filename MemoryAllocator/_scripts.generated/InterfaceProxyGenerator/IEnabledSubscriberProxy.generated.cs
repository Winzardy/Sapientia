using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IEnabledSubscriberProxy : IProxy
	{
		public static readonly ProxyId ProxyId = 15;
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

		public delegate void OnEntityEnabledDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, in Sapientia.MemoryAllocator.State.Entity callbackReceiver);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void OnEntityEnabled(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, in Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, OnEntityEnabledDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, in callbackReceiver);
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

	public static unsafe class IEnabledSubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void OnEntityEnabled(this in UnsafeProxyPtr<IEnabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, in Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			__proxyPtr.proxy.OnEntityEnabled(__proxyPtr.GetPtr().ptr, worldState, in callbackReceiver);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void OnEntityEnabled(this ref ProxyPtr<IEnabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, in Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			__proxyPtr.proxy.OnEntityEnabled(__proxyPtr.GetPtr(__worldState).ptr, worldState, in callbackReceiver);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void OnEntityEnabled(this ref ProxyEvent<IEnabledSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, in Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.OnEntityEnabled(__proxyPtr.GetPtr(__worldState).ptr, worldState, in callbackReceiver);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this in UnsafeProxyPtr<IEnabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr().ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IEnabledSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IEnabledSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			foreach (ref var __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
			}
		}

	}

	public unsafe struct IEnabledSubscriberProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.IEnabledSubscriber
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IEnabledSubscriberProxyExt` найдите `usages` методов `OnEntityEnabled`
		private static void OnEntityEnabled(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, in Sapientia.MemoryAllocator.State.Entity callbackReceiver)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.OnEntityEnabled(worldState, in callbackReceiver);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateOnEntityEnabledDelegate()
		{
			return new IEnabledSubscriberProxy.OnEntityEnabledDelegate(OnEntityEnabled);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IEnabledSubscriberProxyExt` найдите `usages` методов `ProxyDispose`
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
			return new IEnabledSubscriberProxy.ProxyDisposeDelegate(ProxyDispose);
		}
	}
}
