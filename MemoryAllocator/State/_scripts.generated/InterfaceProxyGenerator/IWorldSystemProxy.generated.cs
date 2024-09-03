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

		internal delegate System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystemsDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 0);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<GetSystemsDelegate>(__compiledMethod.functionPointer);
			return __method.Invoke(__executorPtr);
		}

		internal delegate void UpdateDelegate(void* __executorPtr, System.Single deltaTime);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Update(void* __executorPtr, System.Single deltaTime)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 1);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<UpdateDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, deltaTime);
		}

		internal delegate void SetAllocatorIdDelegate(void* __executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void SetAllocatorId(void* __executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 2);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<SetAllocatorIdDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr, allocatorId);
		}

		internal delegate void InitializeDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 3);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

		internal delegate void LateInitializeDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 4);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<LateInitializeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

		internal delegate void StartDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 5);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<StartDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

		internal delegate void DisposeDelegate(void* __executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* __executorPtr)
		{
			var __compiledMethod = IndexedTypes.GetCompiledMethod(this._firstDelegateIndex + 6);
			var __method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<DisposeDelegate>(__compiledMethod.functionPointer);
			__method.Invoke(__executorPtr);
		}

	}

	public static unsafe class IWorldSystemProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(this ProxyRef<IWorldSystemProxy> __proxyRef)
		{
			return __proxyRef.proxy.GetSystems(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent)
		{
			System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> __result = default;
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__result = __proxyRef->proxy.GetSystems(__proxyRef->GetPtr());
			}
			return __result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> __result = default;
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__result = __proxyRef->proxy.GetSystems(__proxyRef->GetPtr());
			}
			return __result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ProxyRef<IWorldSystemProxy> __proxyRef, System.Single deltaTime)
		{
			__proxyRef.proxy.Update(__proxyRef.GetPtr(), deltaTime);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, System.Single deltaTime)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.Update(__proxyRef->GetPtr(), deltaTime);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, System.Single deltaTime)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.Update(__proxyRef->GetPtr(), deltaTime);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ProxyRef<IWorldSystemProxy> __proxyRef, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			__proxyRef.proxy.SetAllocatorId(__proxyRef.GetPtr(), allocatorId);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.SetAllocatorId(__proxyRef->GetPtr(), allocatorId);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.SetAllocatorId(__proxyRef->GetPtr(), allocatorId);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ProxyRef<IWorldSystemProxy> __proxyRef)
		{
			__proxyRef.proxy.Initialize(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.Initialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.Initialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ProxyRef<IWorldSystemProxy> __proxyRef)
		{
			__proxyRef.proxy.LateInitialize(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.LateInitialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.LateInitialize(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ProxyRef<IWorldSystemProxy> __proxyRef)
		{
			__proxyRef.proxy.Start(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.Start(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.Start(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ProxyRef<IWorldSystemProxy> __proxyRef)
		{
			__proxyRef.proxy.Dispose(__proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable())
			{
				__proxyRef->proxy.Dispose(__proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.Allocator* __allocator)
		{
			foreach (ProxyRef<IWorldSystemProxy>* __proxyRef in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyRef->proxy.Dispose(__proxyRef->GetPtr());
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
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.GetSystemsDelegate))]
		private static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(void* executorPtr)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			return __source.GetSystems();
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static CompiledMethod CompileGetSystems()
		{
			return CompiledMethod.Create<IWorldSystemProxy.GetSystemsDelegate>(GetSystems);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.UpdateDelegate))]
		private static void Update(void* executorPtr, System.Single deltaTime)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			__source.Update(deltaTime);
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
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.SetAllocatorIdDelegate))]
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
			return CompiledMethod.Create<IWorldSystemProxy.SetAllocatorIdDelegate>(SetAllocatorId);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.InitializeDelegate))]
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
			return CompiledMethod.Create<IWorldSystemProxy.InitializeDelegate>(Initialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.LateInitializeDelegate))]
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
			return CompiledMethod.Create<IWorldSystemProxy.LateInitializeDelegate>(LateInitialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.StartDelegate))]
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
			return CompiledMethod.Create<IWorldSystemProxy.StartDelegate>(Start);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldSystemProxy.DisposeDelegate))]
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
			return CompiledMethod.Create<IWorldSystemProxy.DisposeDelegate>(Dispose);
		}
	}
}
