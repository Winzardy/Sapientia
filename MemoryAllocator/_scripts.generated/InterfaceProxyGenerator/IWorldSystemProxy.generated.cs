using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IWorldSystemProxy : IProxy
	{
		public static readonly ProxyId ProxyId = 12;
		ProxyId IProxy.ProxyId
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => ProxyId;
		}

		private DelegateIndex _firstDelegateIndex;
		DelegateIndex IProxy.FirstDelegateIndex
		{
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			get => _firstDelegateIndex;
			[System.Runtime.CompilerServices.MethodImplAttribute(256)]
			set => _firstDelegateIndex = value;
		}

		public delegate void UpdateDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self, System.Single deltaTime);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Update(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self, System.Single deltaTime)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, UpdateDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, self, deltaTime);
		}

		public delegate void LateUpdateDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateUpdate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 1);
			var __method = UnsafeExt.As<Delegate, LateUpdateDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, self);
		}

		public delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 2);
			var __method = UnsafeExt.As<Delegate, ProxyDisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState);
		}

		public delegate void InitializeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Initialize(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 3);
			var __method = UnsafeExt.As<Delegate, InitializeDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, self);
		}

		public delegate void LateInitializeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void LateInitialize(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 4);
			var __method = UnsafeExt.As<Delegate, LateInitializeDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, self);
		}

		public delegate void StartDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Start(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 5);
			var __method = UnsafeExt.As<Delegate, StartDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, self);
		}

		public delegate void DisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void Dispose(void* __executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 6);
			var __method = UnsafeExt.As<Delegate, DisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, worldState, self);
		}

	}

	public static unsafe class IWorldSystemProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this in UnsafeProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self, System.Single deltaTime)
		{
			__proxyPtr.proxy.Update(__proxyPtr.GetPtr().ptr, worldState, self, deltaTime);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self, System.Single deltaTime)
		{
			__proxyPtr.proxy.Update(__proxyPtr.GetPtr(__worldState).ptr, worldState, self, deltaTime);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Update(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self, System.Single deltaTime)
		{
			foreach (ref ProxyPtr<IWorldSystemProxy> __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.Update(__proxyPtr.GetPtr(__worldState).ptr, worldState, self, deltaTime);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateUpdate(this in UnsafeProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.LateUpdate(__proxyPtr.GetPtr().ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateUpdate(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.LateUpdate(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateUpdate(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			foreach (ref ProxyPtr<IWorldSystemProxy> __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.LateUpdate(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this in UnsafeProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr().ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState)
		{
			foreach (ref ProxyPtr<IWorldSystemProxy> __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__worldState).ptr, worldState);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this in UnsafeProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr().ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Initialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			foreach (ref ProxyPtr<IWorldSystemProxy> __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.Initialize(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this in UnsafeProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr().ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void LateInitialize(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			foreach (ref ProxyPtr<IWorldSystemProxy> __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.LateInitialize(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this in UnsafeProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr().ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.Start(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Start(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			foreach (ref ProxyPtr<IWorldSystemProxy> __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.Start(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this in UnsafeProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr().ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyPtr<IWorldSystemProxy> __proxyPtr, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void Dispose(this ref ProxyEvent<IWorldSystemProxy> __proxyEvent, Sapientia.MemoryAllocator.WorldState __worldState, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			foreach (ref ProxyPtr<IWorldSystemProxy> __proxyPtr in __proxyEvent.GetEnumerable(__worldState))
			{
				__proxyPtr.proxy.Dispose(__proxyPtr.GetPtr(__worldState).ptr, worldState, self);
			}
		}

	}

	public unsafe struct IWorldSystemProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.IWorldSystem
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IWorldSystemProxyExt` найдите `usages` методов `Update`
		private static void Update(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self, System.Single deltaTime)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Update(worldState, self, deltaTime);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateUpdateDelegate()
		{
			return new IWorldSystemProxy.UpdateDelegate(Update);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IWorldSystemProxyExt` найдите `usages` методов `LateUpdate`
		private static void LateUpdate(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.LateUpdate(worldState, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateLateUpdateDelegate()
		{
			return new IWorldSystemProxy.LateUpdateDelegate(LateUpdate);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IWorldSystemProxyExt` найдите `usages` методов `ProxyDispose`
		private static void ProxyDispose(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.ProxyDispose(worldState);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateProxyDisposeDelegate()
		{
			return new IWorldSystemProxy.ProxyDisposeDelegate(ProxyDispose);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IWorldSystemProxyExt` найдите `usages` методов `Initialize`
		private static void Initialize(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Initialize(worldState, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateInitializeDelegate()
		{
			return new IWorldSystemProxy.InitializeDelegate(Initialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IWorldSystemProxyExt` найдите `usages` методов `LateInitialize`
		private static void LateInitialize(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.LateInitialize(worldState, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateLateInitializeDelegate()
		{
			return new IWorldSystemProxy.LateInitializeDelegate(LateInitialize);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IWorldSystemProxyExt` найдите `usages` методов `Start`
		private static void Start(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Start(worldState, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateStartDelegate()
		{
			return new IWorldSystemProxy.StartDelegate(Start);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		// Чтобы найти дальнейшие `usages` метода - выше в классе `IWorldSystemProxyExt` найдите `usages` методов `Dispose`
		private static void Dispose(void* executorPtr, Sapientia.MemoryAllocator.WorldState worldState, Sapientia.MemoryAllocator.IndexedPtr self)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.Dispose(worldState, self);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateDisposeDelegate()
		{
			return new IWorldSystemProxy.DisposeDelegate(Dispose);
		}
	}
}
