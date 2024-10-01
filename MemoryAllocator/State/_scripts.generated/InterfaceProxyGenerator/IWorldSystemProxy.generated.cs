using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IWorldSystemProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 5;
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

		internal delegate void UpdateDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr, System.Single deltaTime);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Update(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr, System.Single deltaTime)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<UpdateDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, statePartPtr, deltaTime);
		}

		internal delegate void InitializeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 1);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, statePartPtr);
		}

		internal delegate void LateInitializeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 2);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<LateInitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, statePartPtr);
		}

		internal delegate void StartDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 3);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<StartDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, statePartPtr);
		}

		internal delegate void DisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* __executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 4);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<DisposeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocator, statePartPtr);
		}

	}

	public static unsafe class IWorldSystemProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr, System.Single deltaTime)
		{
			__proxyPtr.proxy.Update(__proxyPtr.GetPtr(), allocator, statePartPtr, deltaTime);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr, System.Single deltaTime)
		{
			__proxyPtr.proxy.Update(__proxyPtr.GetPtr(__allocator), allocator, statePartPtr, deltaTime);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr, System.Single deltaTime)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Update(__proxyPtr->GetPtr(), allocator, statePartPtr, deltaTime);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr, System.Single deltaTime)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Update(__proxyPtr->GetPtr(__allocator), allocator, statePartPtr, deltaTime);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr(), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr(__allocator), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Initialize(__proxyPtr->GetPtr(), allocator, statePartPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Initialize(__proxyPtr->GetPtr(__allocator), allocator, statePartPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr(), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr(__allocator), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.LateInitialize(__proxyPtr->GetPtr(), allocator, statePartPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.LateInitialize(__proxyPtr->GetPtr(__allocator), allocator, statePartPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr(), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr(__allocator), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Start(__proxyPtr->GetPtr(), allocator, statePartPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Start(__proxyPtr->GetPtr(__allocator), allocator, statePartPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr(), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr(__allocator), allocator, statePartPtr);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Dispose(__proxyPtr->GetPtr(), allocator, statePartPtr);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			foreach (ProxyPtr<IWorldSystemProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Dispose(__proxyPtr->GetPtr(__allocator), allocator, statePartPtr);
			}
		}

	}

	public unsafe struct IWorldSystemProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.NewWorld.IWorldSystem
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.UpdateDelegate))]
		private static void Update(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr, System.Single deltaTime)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Update(allocator, statePartPtr, deltaTime);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileUpdate()
		{
			return CompiledMethod.Create<IWorldSystemProxy.UpdateDelegate>(Update);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.InitializeDelegate))]
		private static void Initialize(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Initialize(allocator, statePartPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileInitialize()
		{
			return CompiledMethod.Create<IWorldSystemProxy.InitializeDelegate>(Initialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.LateInitializeDelegate))]
		private static void LateInitialize(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.LateInitialize(allocator, statePartPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileLateInitialize()
		{
			return CompiledMethod.Create<IWorldSystemProxy.LateInitializeDelegate>(LateInitialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.StartDelegate))]
		private static void Start(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Start(allocator, statePartPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileStart()
		{
			return CompiledMethod.Create<IWorldSystemProxy.StartDelegate>(Start);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.DisposeDelegate))]
		private static void Dispose(void* executorPtr, Sapientia.MemoryAllocator.Allocator* allocator, Sapientia.MemoryAllocator.Data.IndexedPtr statePartPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Dispose(allocator, statePartPtr);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileDispose()
		{
			return CompiledMethod.Create<IWorldSystemProxy.DisposeDelegate>(Dispose);
		}
	}
}
