using System;
using System.Collections.Generic;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IWorldElementProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 4;
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

		internal delegate void SetAllocatorIdDelegate(void* executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void SetAllocatorId(void* executorPtr, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 0);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<SetAllocatorIdDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr, allocatorId);
		}

		internal delegate void InitializeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 1);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<InitializeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void LateInitializeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 2);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<LateInitializeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void StartDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 3);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<StartDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

		internal delegate void DisposeDelegate(void* executorPtr);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* executorPtr)
		{
			var compiledMethod = IndexedTypes.GetCompiledMethod(_firstDelegateIndex + 4);
			var method = System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer<DisposeDelegate>(compiledMethod.functionPointer);
			method.Invoke(executorPtr);
		}

	}

	public static unsafe class IWorldElementProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ProxyRef<IWorldElementProxy> proxyRef, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			proxyRef.proxy.SetAllocatorId(proxyRef.GetPtr(), allocatorId);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void SetAllocatorId(this ref ProxyEvent<IWorldElementProxy> proxyEvent, Sapientia.MemoryAllocator.AllocatorId allocatorId)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldElementProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.SetAllocatorId(proxyRef->GetPtr(), allocatorId);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ProxyRef<IWorldElementProxy> proxyRef)
		{
			proxyRef.proxy.Initialize(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldElementProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldElementProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Initialize(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ProxyRef<IWorldElementProxy> proxyRef)
		{
			proxyRef.proxy.LateInitialize(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldElementProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldElementProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.LateInitialize(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ProxyRef<IWorldElementProxy> proxyRef)
		{
			proxyRef.proxy.Start(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldElementProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldElementProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Start(proxyRef->GetPtr());
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ProxyRef<IWorldElementProxy> proxyRef)
		{
			proxyRef.proxy.Dispose(proxyRef.GetPtr());
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldElementProxy> proxyEvent)
		{
			ref var allocator = ref proxyEvent.GetAllocator();
			foreach (ProxyRef<IWorldElementProxy>* proxyRef in proxyEvent.GetEnumerable(allocator))
			{
				proxyRef->proxy.Dispose(proxyRef->GetPtr());
			}
		}

	}

	public unsafe struct IWorldElementProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.NewWorld.IWorldElement
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.SetAllocatorIdDelegate))]
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
			return CompiledMethod.Create<IWorldElementProxy.SetAllocatorIdDelegate>(SetAllocatorId);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.InitializeDelegate))]
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
			return CompiledMethod.Create<IWorldElementProxy.InitializeDelegate>(Initialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.LateInitializeDelegate))]
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
			return CompiledMethod.Create<IWorldElementProxy.LateInitializeDelegate>(LateInitialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.StartDelegate))]
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
			return CompiledMethod.Create<IWorldElementProxy.StartDelegate>(Start);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.DisposeDelegate))]
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
			return CompiledMethod.Create<IWorldElementProxy.DisposeDelegate>(Dispose);
		}
	}
}
