using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IKillSubscriberProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 1;
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

		internal delegate void EntityKilledDelegate(void* executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityKilled(void* executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 0);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityKilledDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, in entity);
		}

	}

	public static unsafe class IKillSubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ProxyRef<IKillSubscriberProxy> proxyRef, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			proxyRef.proxy.EntityKilled(proxyRef.GetPtr(), in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityKilled(this ref ProxyEvent<IKillSubscriberProxy> proxyEvent, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IKillSubscriberProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.EntityKilled(proxyRef->GetPtr(), in entity);
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.EntityKilled(in entity);
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
