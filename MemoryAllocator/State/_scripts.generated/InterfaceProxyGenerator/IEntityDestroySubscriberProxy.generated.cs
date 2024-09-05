using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IEntityDestroySubscriberProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 3;
		ProxyIndex IProxy.ProxyIndex
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => ProxyIndex;
		}

		private DelegateIndex _firstDelegateIndex;
		DelegateIndex IProxy.FirstDelegateIndex
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => _firstDelegateIndex;
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			set => _firstDelegateIndex = value;
		}

		internal delegate void EntityDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityDestroyedDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, in entity);
		}

	}

	public static unsafe class IEntityDestroySubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyPtr<IEntityDestroySubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			__proxyPtr.proxy.EntityDestroyed(__proxyPtr.GetPtr(), allocator, in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyPtr<IEntityDestroySubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			__proxyPtr.proxy.EntityDestroyed(__proxyPtr.GetPtr(__allocator), allocator, in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyEvent<IEntityDestroySubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			foreach (ProxyPtr<IEntityDestroySubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.EntityDestroyed(__proxyPtr->GetPtr(), allocator, in entity);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyEvent<IEntityDestroySubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			foreach (ProxyPtr<IEntityDestroySubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.EntityDestroyed(__proxyPtr->GetPtr(__allocator), allocator, in entity);
			}
		}

	}

	public unsafe struct IEntityDestroySubscriberProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.NewWorld.IEntityDestroySubscriber
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IEntityDestroySubscriberProxy.EntityDestroyedDelegate))]
		private static void EntityDestroyed(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityDestroyed(allocator, in entity);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileEntityDestroyed()
		{
			return CompiledMethod.Create<IEntityDestroySubscriberProxy.EntityDestroyedDelegate>(EntityDestroyed);
		}
	}
}
