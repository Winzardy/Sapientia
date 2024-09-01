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

		internal delegate System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystemsDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 0);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<GetSystemsDelegate>(compiledMethod.functionPointer);
			return method.Invoke(executorPtr);
		}

		internal delegate void UpdateDelegate(void* executorPtr, System.Single deltaTime);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Update(void* executorPtr, System.Single deltaTime)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 1);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<UpdateDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, deltaTime);
		}

		internal delegate void SetAllocatorIdDelegate(void* executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void SetAllocatorId(void* executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 2);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<SetAllocatorIdDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, allocatorId);
		}

		internal delegate void InitializeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 3);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void LateInitializeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 4);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<LateInitializeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void StartDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 5);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<StartDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void DisposeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 6);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<DisposeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

	}

	public static unsafe class IWorldSystemProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(this ProxyRef<IWorldSystemProxy> proxyRef)
		{
			return proxyRef.proxy.GetSystems(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> GetSystems(this ref ProxyEvent<IWorldSystemProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldSystemProxy>> result = default;
			foreach (ProxyRef<IWorldSystemProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				result = proxyRef->proxy.GetSystems(proxyRef->GetPtr());
			}
			return result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ProxyRef<IWorldSystemProxy> proxyRef, System.Single deltaTime)
		{
			proxyRef.proxy.Update(proxyRef.GetPtr(), deltaTime);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyEvent<IWorldSystemProxy> proxyEvent, System.Single deltaTime)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldSystemProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Update(proxyRef->GetPtr(), deltaTime);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ProxyRef<IWorldSystemProxy> proxyRef, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			proxyRef.proxy.SetAllocatorId(proxyRef.GetPtr(), allocatorId);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ref ProxyEvent<IWorldSystemProxy> proxyEvent, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldSystemProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.SetAllocatorId(proxyRef->GetPtr(), allocatorId);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ProxyRef<IWorldSystemProxy> proxyRef)
		{
			proxyRef.proxy.Initialize(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldSystemProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldSystemProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Initialize(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ProxyRef<IWorldSystemProxy> proxyRef)
		{
			proxyRef.proxy.LateInitialize(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldSystemProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldSystemProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.LateInitialize(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ProxyRef<IWorldSystemProxy> proxyRef)
		{
			proxyRef.proxy.Start(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldSystemProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldSystemProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Start(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ProxyRef<IWorldSystemProxy> proxyRef)
		{
			proxyRef.proxy.Dispose(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldSystemProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldSystemProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Dispose(proxyRef->GetPtr());
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			return @source.GetSystems();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Update(deltaTime);
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.SetAllocatorId(allocatorId);
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Initialize();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.LateInitialize();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Start();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Dispose();
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
