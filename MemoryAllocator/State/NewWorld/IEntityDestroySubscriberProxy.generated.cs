using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IEntityDestroySubscriberProxy : IProxy
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

		internal delegate void EntityDestroyedDelegate(void* executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityDestroyed(void* executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 0);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityDestroyedDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, in entity);
		}

	}

	public static unsafe class IEntityDestroySubscriberProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ProxyRef<IEntityDestroySubscriberProxy> proxyRef, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			proxyRef.proxy.EntityDestroyed(proxyRef.GetPtr(), in entity);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyEvent<IEntityDestroySubscriberProxy> proxyEvent, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IEntityDestroySubscriberProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.EntityDestroyed(proxyRef->GetPtr(), in entity);
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
		private static void EntityDestroyed(void* executorPtr, in Sapientia.MemoryAllocator.State.NewWorld.Entity entity)
		{
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.EntityDestroyed(in entity);
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
