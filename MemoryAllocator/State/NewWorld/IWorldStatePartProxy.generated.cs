using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IWorldStatePartProxy : IProxy
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

		internal delegate System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStatePartsDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 0);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<GetStatePartsDelegate>(compiledMethod.functionPointer);
			return method.Invoke(executorPtr);
		}

		internal delegate void SetAllocatorIdDelegate(void* executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void SetAllocatorId(void* executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 1);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<SetAllocatorIdDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, allocatorId);
		}

		internal delegate void InitializeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 2);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void LateInitializeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 3);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<LateInitializeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void StartDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 4);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<StartDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void DisposeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 5);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<DisposeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

	}

	public static unsafe class IWorldStatePartProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(this ProxyRef<IWorldStatePartProxy> proxyRef)
		{
			return proxyRef.proxy.GetStateParts(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> GetStateParts(this ref ProxyEvent<IWorldStatePartProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			System.Collections.Generic.IEnumerable<Sapientia.MemoryAllocator.Data.ProxyRef<Sapientia.TypeIndexer.IWorldStatePartProxy>> result = default;
			foreach (ProxyRef<IWorldStatePartProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				result = proxyRef->proxy.GetStateParts(proxyRef->GetPtr());
			}
			return result;
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ProxyRef<IWorldStatePartProxy> proxyRef, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			proxyRef.proxy.SetAllocatorId(proxyRef.GetPtr(), allocatorId);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ref ProxyEvent<IWorldStatePartProxy> proxyEvent, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldStatePartProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.SetAllocatorId(proxyRef->GetPtr(), allocatorId);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ProxyRef<IWorldStatePartProxy> proxyRef)
		{
			proxyRef.proxy.Initialize(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldStatePartProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldStatePartProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Initialize(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ProxyRef<IWorldStatePartProxy> proxyRef)
		{
			proxyRef.proxy.LateInitialize(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldStatePartProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldStatePartProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.LateInitialize(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ProxyRef<IWorldStatePartProxy> proxyRef)
		{
			proxyRef.proxy.Start(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldStatePartProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldStatePartProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Start(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ProxyRef<IWorldStatePartProxy> proxyRef)
		{
			proxyRef.proxy.Dispose(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldStatePartProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldStatePartProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Dispose(proxyRef->GetPtr());
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			return @source.GetStateParts();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.SetAllocatorId(allocatorId);
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Initialize();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.LateInitialize();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Start();
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
			ref var @source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
			@source.Dispose();
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
