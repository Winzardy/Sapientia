using System;
using System.Collections.Generic;
using Sapientia.Extensions;
using Sapientia.MemoryAllocator;

namespace Sapientia.TypeIndexer
{
	public unsafe struct IElementDestroyHandlerProxy : IProxy
	{
		public static readonly ProxyId ProxyId = 10;
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

		public delegate void EntityPtrArrayDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.World world, void** elementsPtr, System.Int32 count);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityPtrArrayDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.World world, void** elementsPtr, System.Int32 count)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 0);
			var __method = UnsafeExt.As<Delegate, EntityPtrArrayDestroyedDelegate>(__delegate);
			__method.Invoke(__executorPtr, world, elementsPtr, count);
		}

		public delegate void EntityArrayDestroyedDelegate(void* __executorPtr, Sapientia.MemoryAllocator.World world, void* elementsPtr, System.Int32 count);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void EntityArrayDestroyed(void* __executorPtr, Sapientia.MemoryAllocator.World world, void* elementsPtr, System.Int32 count)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 1);
			var __method = UnsafeExt.As<Delegate, EntityArrayDestroyedDelegate>(__delegate);
			__method.Invoke(__executorPtr, world, elementsPtr, count);
		}

		public delegate void ProxyDisposeDelegate(void* __executorPtr, Sapientia.MemoryAllocator.World world);
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public readonly void ProxyDispose(void* __executorPtr, Sapientia.MemoryAllocator.World world)
		{
			var __delegate = IndexedTypes.GetDelegate(this._firstDelegateIndex + 2);
			var __method = UnsafeExt.As<Delegate, ProxyDisposeDelegate>(__delegate);
			__method.Invoke(__executorPtr, world);
		}

	}

	public static unsafe class IElementDestroyHandlerProxyExt
	{
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityPtrArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world, void** elementsPtr, System.Int32 count)
		{
			__proxyPtr.proxy.EntityPtrArrayDestroyed(__proxyPtr.GetPtr(__world).ptr, world, elementsPtr, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityPtrArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world, void** elementsPtr, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__world))
			{
				__proxyPtr->proxy.EntityPtrArrayDestroyed(__proxyPtr->GetPtr(__world).ptr, world, elementsPtr, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world, void* elementsPtr, System.Int32 count)
		{
			__proxyPtr.proxy.EntityArrayDestroyed(__proxyPtr.GetPtr(__world).ptr, world, elementsPtr, count);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void EntityArrayDestroyed(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world, void* elementsPtr, System.Int32 count)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__world))
			{
				__proxyPtr->proxy.EntityArrayDestroyed(__proxyPtr->GetPtr(__world).ptr, world, elementsPtr, count);
			}
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyPtr<IElementDestroyHandlerProxy> __proxyPtr, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world)
		{
			__proxyPtr.proxy.ProxyDispose(__proxyPtr.GetPtr(__world).ptr, world);
		}

		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static void ProxyDispose(this ref ProxyEvent<IElementDestroyHandlerProxy> __proxyEvent, Sapientia.MemoryAllocator.World __world, Sapientia.MemoryAllocator.World world)
		{
			foreach (ProxyPtr<IElementDestroyHandlerProxy>* __proxyPtr in __proxyEvent.GetEnumerable(__world))
			{
				__proxyPtr->proxy.ProxyDispose(__proxyPtr->GetPtr(__world).ptr, world);
			}
		}

	}

	public unsafe struct IElementDestroyHandlerProxy<TSource> where TSource: struct, Sapientia.MemoryAllocator.State.IElementDestroyHandler
	{
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		private static void EntityPtrArrayDestroyed(void* executorPtr, Sapientia.MemoryAllocator.World world, void** elementsPtr, System.Int32 count)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityPtrArrayDestroyed(world, elementsPtr, count);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateEntityPtrArrayDestroyedDelegate()
		{
			return new IElementDestroyHandlerProxy.EntityPtrArrayDestroyedDelegate(EntityPtrArrayDestroyed);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		private static void EntityArrayDestroyed(void* executorPtr, Sapientia.MemoryAllocator.World world, void* elementsPtr, System.Int32 count)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.EntityArrayDestroyed(world, elementsPtr, count);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateEntityArrayDestroyedDelegate()
		{
			return new IElementDestroyHandlerProxy.EntityArrayDestroyedDelegate(EntityArrayDestroyed);
		}
#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#if BURST
		[Unity.Burst.BurstCompileAttribute(Unity.Burst.FloatPrecision.High, Unity.Burst.FloatMode.Deterministic, CompileSynchronously = true, Debug = false)]
#endif
#endif
		private static void ProxyDispose(void* executorPtr, Sapientia.MemoryAllocator.World world)
		{
			ref var __source = ref Sapientia.Extensions.UnsafeExt.AsRef<TSource>(executorPtr);
#if PROXY_REFACTORING
#else
			__source.ProxyDispose(world);
#endif
		}

#if UNITY_5_3_OR_NEWER
		[UnityEngine.Scripting.Preserve]
#endif
		[System.Runtime.CompilerServices.MethodImplAttribute(256)]
		public static Delegate CreateProxyDisposeDelegate()
		{
			return new IElementDestroyHandlerProxy.ProxyDisposeDelegate(ProxyDispose);
		}
	}
}
