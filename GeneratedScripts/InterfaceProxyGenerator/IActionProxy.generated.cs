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

		internal delegate void InvokeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Invoke(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 0);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InvokeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

	}

	public static unsafe class IActionProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Invoke(this ProxyRef<IActionProxy> proxyRef)
		{
			proxyRef.proxy.Invoke(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Invoke(this ref ProxyEvent<IActionProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IActionProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Invoke(proxyRef->GetPtr());
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Invoke();
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
