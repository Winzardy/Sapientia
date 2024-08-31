using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IElementDestroyHandlerProxy : IProxy
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

		internal delegate void EntityDestroyedDelegate(void* executorPtr, void* element);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityDestroyed(void* executorPtr, void* element)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 0);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityDestroyedDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, element);
		}

		internal delegate void EntityArrayDestroyedDelegate(void* executorPtr, void* element, System.UInt32 count);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityArrayDestroyed(void* executorPtr, void* element, System.UInt32 count)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 1);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityArrayDestroyedDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, element, count);
		}

	}

	public static unsafe class IElementDestroyHandlerProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ProxyRef<IElementDestroyHandlerProxy> proxyRef, void* element)
		{
			proxyRef.proxy.EntityDestroyed(proxyRef.GetPtr(), element);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> proxyEvent, void* element)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IElementDestroyHandlerProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.EntityDestroyed(proxyRef->GetPtr(), element);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ProxyRef<IElementDestroyHandlerProxy> proxyRef, void* element, System.UInt32 count)
		{
			proxyRef.proxy.EntityArrayDestroyed(proxyRef.GetPtr(), element, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> proxyEvent, void* element, System.UInt32 count)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IElementDestroyHandlerProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.EntityArrayDestroyed(proxyRef->GetPtr(), element, count);
			}
		}

	}

	public unsafe struct IElementDestroyHandlerProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.NewWorld.IElementDestroyHandler
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityDestroyedDelegate))]
		private static void EntityDestroyed(void* executorPtr, void* element)
		{
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.EntityDestroyed(element);
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileEntityDestroyed()
		{
			return CompiledMethod.Create<IElementDestroyHandlerProxy.EntityDestroyedDelegate>(EntityDestroyed);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityArrayDestroyedDelegate))]
		private static void EntityArrayDestroyed(void* executorPtr, void* element, System.UInt32 count)
		{
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.EntityArrayDestroyed(element, count);
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileEntityArrayDestroyed()
		{
			return CompiledMethod.Create<IElementDestroyHandlerProxy.EntityArrayDestroyedDelegate>(EntityArrayDestroyed);
		}
	}
}
