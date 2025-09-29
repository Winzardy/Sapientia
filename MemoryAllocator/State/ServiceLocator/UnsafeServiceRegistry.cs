using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Data;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	public struct UnsafeServiceRegistry : IDisposable
	{
		private UnsafeDictionary<ServiceRegistryContext, SafePtr> _typeToPtr;

		public void Clear()
		{
			foreach (var entry in _typeToPtr)
			{
				MemoryExt.MemFree(entry.value);
			}
			_typeToPtr.Clear();
		}

		public void Dispose()
		{
			foreach (var entry in _typeToPtr)
			{
				MemoryExt.MemFree(entry.value);
			}
			_typeToPtr.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetPtr<T>(ServiceRegistryContext context) where T : unmanaged
		{
			return _typeToPtr[context];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetPtr<T>(out SafePtr<T> value) where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			var result = _typeToPtr.TryGetValue(context, out var ptr);
			value = ptr;

			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetPtr<T>() where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			return _typeToPtr[context];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>(ServiceRegistryContext context) where T : unmanaged
		{
			return ref GetPtr<T>(context).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>() where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			return ref GetPtr<T>(context).Value();
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>(WorldState worldState) where T: unmanaged, IInitializableService
		{
			var typeIndex = TypeIndex.Create<T>();
			return ref GetOrCreate<T>(worldState, typeIndex);
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged, IInitializableService
		{
			ref var service = ref GetOrCreatePtr<T>(context, out var isExist).Value();
			if (!isExist)
				service.Initialize(worldState);

			return ref service;
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>(WorldState worldState) where T: unmanaged, IInitializableService
		{
			var typeIndex = TypeIndex.Create<T>();
			return GetOrCreatePtr<T>(worldState, typeIndex);
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrRegister`, который просто регистрирует)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>(WorldState worldState, ServiceRegistryContext context) where T: unmanaged, IInitializableService
		{
			var servicePtr = GetOrCreatePtr<T>(context, out var isExist);
			if (!isExist)
				servicePtr.Value().Initialize(worldState);

			return servicePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>(ServiceRegistryContext context, out bool isExist) where T : unmanaged
		{
			var value = _typeToPtr.GetValue(context, out isExist);
			if (isExist)
				return value;

			value = MemoryExt.MemAlloc<T>();
			_typeToPtr.Add(context, value);

			return value;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>(ServiceRegistryContext context) where T : unmanaged
		{
			return GetOrCreatePtr<T>(context, out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>() where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			return GetOrCreatePtr<T>(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>(out bool isExist) where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			return GetOrCreatePtr<T>(context, out isExist);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>(ServiceRegistryContext context) where T : unmanaged
		{
			return ref GetOrCreatePtr<T>(context).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>() where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			return ref GetOrCreatePtr<T>(context).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>(out ServiceRegistryContext context) where T : unmanaged, IIndexedType
		{
			context = ServiceRegistryContext.Create<T>();
			return ref GetOrCreatePtr<T>(context).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<T>(ServiceRegistryContext context, out T value) where T : unmanaged
		{
			value = default;
			var success = _typeToPtr.Remove(context, out var ptr);
			if (success)
			{
				value = ptr.Value<T>();
				MemoryExt.MemFree(ptr);
			}
			return success;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove(ServiceRegistryContext context)
		{
			var success = _typeToPtr.Remove(context, out var ptr);
			if (success)
			{
				MemoryExt.MemFree(ptr);
			}
			return success;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<T>() where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			return Remove(context);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<T>(out T value) where T : unmanaged, IIndexedType
		{
			var context = ServiceRegistryContext.Create<T>();
			return Remove(context, out value);
		}
	}
}
