using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IKillSubscriberProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 2;
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

		internal delegate void EntityKilledDelegate(void* __executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityKilled(void* __executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityKilledDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, in entity);
		}

	}

	public static unsafe class IKillSubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ProxyRef<IKillSubscriberProxy> __proxyRef, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			__proxyRef.proxy.EntityKilled(__proxyRef.GetPtr(), in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			foreach (ProxyRef<IKillSubscriberProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.EntityKilled(__proxyRef->GetPtr(), in entity);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyEvent<IKillSubscriberProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			foreach (ProxyRef<IKillSubscriberProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.EntityKilled(__proxyRef->GetPtr(), in entity);
			}
		}

	}

	public unsafe struct IKillSubscriberProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.NewWorld.IKillSubscriber
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IKillSubscriberProxy.EntityKilledDelegate))]
		private static void EntityKilled(void* executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			__source.EntityKilled(in entity);
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileEntityKilled()
		{
			return CompiledMethod.Create<IKillSubscriberProxy.EntityKilledDelegate>(EntityKilled);
		}
	}
}
