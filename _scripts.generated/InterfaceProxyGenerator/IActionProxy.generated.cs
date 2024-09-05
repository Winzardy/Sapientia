using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IActionProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 0;
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

		internal delegate void InvokeDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Invoke(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InvokeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

	}

	public static unsafe class IActionProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Invoke(this ref ProxyPtr<IActionProxy> __proxyPtr)
		{
			__proxyPtr.proxy.Invoke(__proxyPtr.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Invoke(this ref ProxyPtr<IActionProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			__proxyPtr.proxy.Invoke(__proxyPtr.GetPtr(__allocator));
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Invoke(this ref ProxyEvent<IActionProxy> __proxyEvent)
		{
			foreach (ProxyPtr<IActionProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Invoke(__proxyPtr->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Invoke(this ref ProxyEvent<IActionProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyPtr<IActionProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Invoke(__proxyPtr->GetPtr(__allocator));
			}
		}

	}

	public unsafe struct IActionProxy<TSource> where TSource: struct, Sapientia.TypeIndexer.BaseProxies.IAction
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IActionProxy.InvokeDelegate))]
		private static void Invoke(void* executorPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Invoke();
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileInvoke()
		{
			return CompiledMethod.Create<IActionProxy.InvokeDelegate>(Invoke);
		}
	}
}
