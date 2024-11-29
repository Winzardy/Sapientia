using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IWorldStatePartProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 6;
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

		internal delegate void InitializeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, elementPtr);
		}

		internal delegate void LateInitializeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 1);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<LateInitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, elementPtr);
		}

		internal delegate void StartDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 2);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<StartDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, elementPtr);
		}

		internal delegate void DisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 3);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<DisposeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, elementPtr);
		}

	}

	public static unsafe class IWorldStatePartProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr(), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr(__allocator), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Initialize(__proxyPtr->GetPtr(), allocator, elementPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Initialize(__proxyPtr->GetPtr(__allocator), allocator, elementPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr(), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr(__allocator), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.LateInitialize(__proxyPtr->GetPtr(), allocator, elementPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.LateInitialize(__proxyPtr->GetPtr(__allocator), allocator, elementPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr(), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr(__allocator), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Start(__proxyPtr->GetPtr(), allocator, elementPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Start(__proxyPtr->GetPtr(__allocator), allocator, elementPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr(), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyPtr<IWorldStatePartProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr(__allocator), allocator, elementPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Dispose(__proxyPtr->GetPtr(), allocator, elementPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			foreach (ProxyPtr<IWorldStatePartProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Dispose(__proxyPtr->GetPtr(__allocator), allocator, elementPtr);
			}
		}

	}

	public unsafe struct IWorldStatePartProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.NewWorld.IWorldStatePart
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityDestroyedDelegate))]
#endif
		private static void Initialize(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Initialize(allocator, elementPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileInitialize()
		{
			return CompiledMethod.Create<IWorldStatePartProxy.InitializeDelegate>(Initialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityDestroyedDelegate))]
#endif
		private static void LateInitialize(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.LateInitialize(allocator, elementPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileLateInitialize()
		{
			return CompiledMethod.Create<IWorldStatePartProxy.LateInitializeDelegate>(LateInitialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityDestroyedDelegate))]
#endif
		private static void Start(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Start(allocator, elementPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileStart()
		{
			return CompiledMethod.Create<IWorldStatePartProxy.StartDelegate>(Start);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IElementDestroyHandlerProxy.EntityDestroyedDelegate))]
#endif
		private static void Dispose(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr elementPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Dispose(allocator, elementPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileDispose()
		{
			return CompiledMethod.Create<IWorldStatePartProxy.DisposeDelegate>(Dispose);
		}
	}
}
