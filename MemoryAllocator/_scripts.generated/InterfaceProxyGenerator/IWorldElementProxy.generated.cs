using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator.Data;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IWorldElementProxy : IProxy
	{
		public static readonly ProxyIndex ProxyIndex = 13;
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

		public delegate void InitializeDelegate(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, InitializeDelegate>(__delegate);
			__method.Invoke(__executorPtr, allocator, self);
		}

		public delegate void LateInitializeDelegate(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 1);
			var __method = UnsafeExt.As<Delegate, LateInitializeDelegate>(__delegate);
			__method.Invoke(__executorPtr, allocator, self);
		}

		public delegate void StartDelegate(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 2);
			var __method = UnsafeExt.As<Delegate, StartDelegate>(__delegate);
			__method.Invoke(__executorPtr, allocator, self);
		}

		public delegate void DisposeDelegate(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 3);
			var __method = UnsafeExt.As<Delegate, DisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, allocator, self);
		}

		public delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 4);
			var __method = UnsafeExt.As<Delegate, ProxyDisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, allocator);
		}

	}

	public static unsafe class IWorldElementProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr().ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr(__allocator).ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Initialize(__proxyPtr->GetPtr().ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Initialize(__proxyPtr->GetPtr(__allocator).ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr().ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr(__allocator).ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.LateInitialize(__proxyPtr->GetPtr().ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.LateInitialize(__proxyPtr->GetPtr(__allocator).ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr().ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr(__allocator).ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Start(__proxyPtr->GetPtr().ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Start(__proxyPtr->GetPtr(__allocator).ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr().ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr(__allocator).ptr, allocator, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.Dispose(__proxyPtr->GetPtr().ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.Dispose(__proxyPtr->GetPtr(__allocator).ptr, allocator, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr().ptr, allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IWorldElementProxy> __proxyPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__allocator).ptr, allocator);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable())
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr().ptr, allocator);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IWorldElementProxy> __proxyEvent, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> __allocator, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator)
		{
			foreach (ProxyPtr<IWorldElementProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__allocator))
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(__allocator).ptr, allocator);
			}
		}

	}

	public unsafe struct IWorldElementProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.IWorldElement
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.InitializeDelegate))]
#endif
		private static void Initialize(void* executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Initialize(allocator, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateInitializeDelegate()
		{
			return new IWorldElementProxy.InitializeDelegate(Initialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.LateInitializeDelegate))]
#endif
		private static void LateInitialize(void* executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.LateInitialize(allocator, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateLateInitializeDelegate()
		{
			return new IWorldElementProxy.LateInitializeDelegate(LateInitialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.StartDelegate))]
#endif
		private static void Start(void* executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Start(allocator, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateStartDelegate()
		{
			return new IWorldElementProxy.StartDelegate(Start);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.DisposeDelegate))]
#endif
		private static void Dispose(void* executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator, Sapientia.MemoryAllocator.Data.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Dispose(allocator, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateDisposeDelegate()
		{
			return new IWorldElementProxy.DisposeDelegate(Dispose);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
		[AOT.MonoPInvokeCallbackAttribute(typeof(IWorldElementProxy.ProxyDisposeDelegate))]
#endif
		private static void ProxyDispose(void* executorPtr, Sapientia.Data.SafePtr<Sapientia.MemoryAllocator.Allocator> allocator)
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
		public static Delegate CreateProxyDisposeDelegate()
		{
			return new IWorldElementProxy.ProxyDisposeDelegate(ProxyDispose);
		}
	}
}
