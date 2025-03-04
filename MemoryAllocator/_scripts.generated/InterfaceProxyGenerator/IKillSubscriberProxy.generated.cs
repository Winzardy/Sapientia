using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IKillSubscriberProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 26;
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

		internal delegate void EntityKilledDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.Entity entity);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityKilled(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityKilledDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, in entity);
		}

		internal delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 1);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<ProxyDisposeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator);
		}

	}

	public static unsafe class IKillSubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyPtr<IKillSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			__proxyPtr.proxy.EntityKilled(__proxyPtr.GetPtr(), allocator, in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyPtr<IKillSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			__proxyPtr.proxy.EntityKilled(__proxyPtr.GetPtr(__allocator), allocator, in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			foreach (ProxyPtr<IKillSubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.EntityKilled(__proxyPtr->GetPtr(), allocator, in entity);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			foreach (ProxyPtr<IKillSubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.EntityKilled(__proxyPtr->GetPtr(__allocator), allocator, in entity);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IKillSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(), allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IKillSubscriberProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__allocator), allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			foreach (ProxyPtr<IKillSubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(), allocator);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			foreach (ProxyPtr<IKillSubscriberProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(__allocator), allocator);
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
		[AOT.MonoPInvokeCallbackAttribute(typeof(IKillSubscriberProxy.EntityKilledDelegate))]
#endif
		private static void EntityKilled(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, in Sapientia.MemoryAllocator.State.Entity entity)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityKilled(allocator, in entity);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileEntityKilled()
		{
			return CompiledMethod.Create<IKillSubscriberProxy.EntityKilledDelegate>(EntityKilled);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IKillSubscriberProxy.ProxyDisposeDelegate))]
#endif
		private static void ProxyDispose(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.ProxyDispose(allocator);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileProxyDispose()
		{
			return CompiledMethod.Create<IKillSubscriberProxy.ProxyDisposeDelegate>(ProxyDispose);
		}
	}
}
