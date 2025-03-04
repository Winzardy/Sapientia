using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IElementDestroyHandlerProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 25;
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

		internal delegate void EntityPtrArrayDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void** elementsPtr, System.Int32 count);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityPtrArrayDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void** elementsPtr, System.Int32 count)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityPtrArrayDestroyedDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, elementsPtr, count);
		}

		internal delegate void EntityArrayDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* elementsPtr, System.Int32 count);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityArrayDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* elementsPtr, System.Int32 count)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 1);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<EntityArrayDestroyedDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, elementsPtr, count);
		}

		internal delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 2);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<ProxyDisposeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator);
		}

	}

	public static unsafe class IElementDestroyHandlerProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityPtrArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, void** elementsPtr, System.Int32 count)
		{
			__proxyPtr.proxy.EntityPtrArrayDestroyed(__proxyPtr.GetPtr(), allocator, elementsPtr, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityPtrArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void** elementsPtr, System.Int32 count)
		{
			__proxyPtr.proxy.EntityPtrArrayDestroyed(__proxyPtr.GetPtr(__allocator), allocator, elementsPtr, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityPtrArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, void** elementsPtr, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.EntityPtrArrayDestroyed(__proxyPtr->GetPtr(), allocator, elementsPtr, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityPtrArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void** elementsPtr, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.EntityPtrArrayDestroyed(__proxyPtr->GetPtr(__allocator), allocator, elementsPtr, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* elementsPtr, System.Int32 count)
		{
			__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr(), allocator, elementsPtr, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void* elementsPtr, System.Int32 count)
		{
			__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr(__allocator), allocator, elementsPtr, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, void* elementsPtr, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.EntityArrayDestroyed(__proxyPtr->GetPtr(), allocator, elementsPtr, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, void* elementsPtr, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.EntityArrayDestroyed(__proxyPtr->GetPtr(__allocator), allocator, elementsPtr, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(), allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__allocator), allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(), allocator);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(__allocator), allocator);
			}
		}

	}

	public unsafe struct IElementDestroyHandlerProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.IElementDestroyHandler
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityPtrArrayDestroyedDelegate))]
#endif
		private static void EntityPtrArrayDestroyed(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void** elementsPtr, System.Int32 count)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityPtrArrayDestroyed(allocator, elementsPtr, count);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileEntityPtrArrayDestroyed()
		{
			return CompiledMethod.Create<IElementDestroyHandlerProxy.EntityPtrArrayDestroyedDelegate>(EntityPtrArrayDestroyed);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityArrayDestroyedDelegate))]
#endif
		private static void EntityArrayDestroyed(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, void* elementsPtr, System.Int32 count)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityArrayDestroyed(allocator, elementsPtr, count);
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
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.ProxyDisposeDelegate))]
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
			return CompiledMethod.Create<IElementDestroyHandlerProxy.ProxyDisposeDelegate>(ProxyDispose);
		}
	}
}
