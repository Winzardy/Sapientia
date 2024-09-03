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

		internal delegate System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStatePartsDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<GetStatePartsDelegate>(__compiledMethod.functionPointer);
			return __method.Invoke(__executorPtr);
		}

		internal delegate void SetAllocatorIdDelegate(void* __executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void SetAllocatorId(void* __executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 1);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<SetAllocatorIdDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocatorId);
		}

		internal delegate void InitializeDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 2);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

		internal delegate void LateInitializeDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 3);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<LateInitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

		internal delegate void StartDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 4);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<StartDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

		internal delegate void DisposeDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 5);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<DisposeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

	}

	public static unsafe class IWorldStatePartProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(this ProxyRef<IWorldStatePartProxy> __proxyRef)
		{
			return __proxyRef.proxy.GetStateParts(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent)
		{
			System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> __result = default;
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__result = __proxyRef->proxy.GetStateParts(__proxyRef->GetPtr());
			}
			return __result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> __result = default;
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__result = __proxyRef->proxy.GetStateParts(__proxyRef->GetPtr());
			}
			return __result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ProxyRef<IWorldStatePartProxy> __proxyRef, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			__proxyRef.proxy.SetAllocatorId(__proxyRef.GetPtr(), allocatorId);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.SetAllocatorId(__proxyRef->GetPtr(), allocatorId);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.SetAllocatorId(__proxyRef->GetPtr(), allocatorId);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ProxyRef<IWorldStatePartProxy> __proxyRef)
		{
			__proxyRef.proxy.Initialize(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.Initialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.Initialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ProxyRef<IWorldStatePartProxy> __proxyRef)
		{
			__proxyRef.proxy.LateInitialize(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.LateInitialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.LateInitialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ProxyRef<IWorldStatePartProxy> __proxyRef)
		{
			__proxyRef.proxy.Start(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.Start(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.Start(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ProxyRef<IWorldStatePartProxy> __proxyRef)
		{
			__proxyRef.proxy.Dispose(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.Dispose(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldStatePartProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldStatePartProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.Dispose(__proxyRef->GetPtr());
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
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldStatePartProxy.GetStatePartsDelegate))]
		private static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(void* executorPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			return __source.GetStateParts();
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileGetStateParts()
		{
			return CompiledMethod.Create<IWorldStatePartProxy.GetStatePartsDelegate>(GetStateParts);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldStatePartProxy.SetAllocatorIdDelegate))]
		private static void SetAllocatorId(void* executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			__source.SetAllocatorId(allocatorId);
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileSetAllocatorId()
		{
			return CompiledMethod.Create<IWorldStatePartProxy.SetAllocatorIdDelegate>(SetAllocatorId);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldStatePartProxy.InitializeDelegate))]
		private static void Initialize(void* executorPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			__source.Initialize();
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
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldStatePartProxy.LateInitializeDelegate))]
		private static void LateInitialize(void* executorPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			__source.LateInitialize();
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
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldStatePartProxy.StartDelegate))]
		private static void Start(void* executorPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			__source.Start();
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
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldStatePartProxy.DisposeDelegate))]
		private static void Dispose(void* executorPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			__source.Dispose();
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
