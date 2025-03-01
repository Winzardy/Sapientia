using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IInterfaceProxyTypeProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 7;
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

		internal delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<ProxyDisposeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator);
		}

	}

	public static unsafe class IInterfaceProxyTypeProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IInterfaceProxyTypeProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(), allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IInterfaceProxyTypeProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__allocator), allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IInterfaceProxyTypeProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			foreach (ProxyPtr<IInterfaceProxyTypeProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(), allocator);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IInterfaceProxyTypeProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator)
		{
			foreach (ProxyPtr<IInterfaceProxyTypeProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(__allocator), allocator);
			}
		}

	}

	public unsafe struct IInterfaceProxyTypeProxy<TSource> where TSource: struct, Sapientia.TypeIndexer.IInterfaceProxyType
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IInterfaceProxyTypeProxy.ProxyDisposeDelegate))]
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
			return CompiledMethod.Create<IInterfaceProxyTypeProxy.ProxyDisposeDelegate>(ProxyDispose);
		}
	}
}
