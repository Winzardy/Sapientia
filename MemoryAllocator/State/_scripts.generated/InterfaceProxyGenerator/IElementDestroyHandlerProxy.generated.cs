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

		internal delegate void EntityDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityDestroyedDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, element);
		}

		internal delegate void EntityArrayDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element, System.Int32 count);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityArrayDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element, System.Int32 count)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 1);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityArrayDestroyedDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, element, count);
		}

	}

	public static unsafe class IElementDestroyHandlerProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element)
		{
			__proxyPtr.proxy.EntityDestroyed(__proxyPtr.GetPtr(), allocator, element);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void* element)
		{
			__proxyPtr.proxy.EntityDestroyed(__proxyPtr.GetPtr(__allocator), allocator, element);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, void* element)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.EntityDestroyed(__proxyPtr->GetPtr(), allocator, element);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void* element)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.EntityDestroyed(__proxyPtr->GetPtr(__allocator), allocator, element);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element, System.Int32 count)
		{
			__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr(), allocator, element, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void* element, System.Int32 count)
		{
			__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr(__allocator), allocator, element, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, void* element, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.EntityArrayDestroyed(__proxyPtr->GetPtr(), allocator, element, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void* element, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.EntityArrayDestroyed(__proxyPtr->GetPtr(__allocator), allocator, element, count);
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
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityDestroyedDelegate))]
#endif
		private static void EntityDestroyed(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityDestroyed(allocator, element);
#endif
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
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityArrayDestroyedDelegate))]
#endif
		private static void EntityArrayDestroyed(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* element, System.Int32 count)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityArrayDestroyed(allocator, element, count);
#endif
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
