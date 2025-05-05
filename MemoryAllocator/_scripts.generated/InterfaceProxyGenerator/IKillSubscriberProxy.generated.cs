using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IKillSubscriberProxy : IProxy
	{
		public static readonly ProxyId ProxyId = 11;
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

		public delegate void EntityKilledDelegate(void* __executorPtr, Sapientia.MemoryAllocator.World world, in Sapientia.MemoryAllocator.State.Entity entity);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityKilled(void* __executorPtr, Sapientia.MemoryAllocator.World world, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, EntityKilledDelegate>(__delegate);
			__method.Invoke(__executorPtr, world, in entity);
		}

		public delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.World world);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.World world)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 1);
			var __method = UnsafeExt.As<Delegate, ProxyDisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, world);
		}

	}

	public static unsafe class IKillSubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyPtr<IKillSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			__proxyPtr.proxy.EntityKilled(__proxyPtr.GetPtr(__world).ptr, world, in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			foreach (ProxyPtr<IKillSubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__world))
			{
				__proxyPtr->proxy.EntityKilled(__proxyPtr->GetPtr(__world).ptr, world, in entity);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IKillSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__world).ptr, world);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world)
		{
			foreach (ProxyPtr<IKillSubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__world))
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(__world).ptr, world);
			}
		}

	}

	public unsafe struct IKillSubscriberProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.IKillSubscriber
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		private static void EntityKilled(void* executorPtr, Sapientia.MemoryAllocator.World world, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityKilled(world, in entity);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateEntityKilledDelegate()
		{
			return new IKillSubscriberProxy.EntityKilledDelegate(EntityKilled);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		private static void ProxyDispose(void* executorPtr, Sapientia.MemoryAllocator.World world)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.ProxyDispose(world);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateProxyDisposeDelegate()
		{
			return new IKillSubscriberProxy.ProxyDisposeDelegate(ProxyDispose);
		}
	}
}
