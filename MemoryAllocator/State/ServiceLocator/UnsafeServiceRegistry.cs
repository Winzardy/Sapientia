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
		private UnsafeArray<SafePtr> _typeToPtr;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void EnsureInitialized()
		{
			if (!_typeToPtr.IsCreated)
				_typeToPtr = new UnsafeArray<SafePtr>(TypeId<IWorldLocalUnmanagedService>.Count);
		}

		public void Clear()
		{
			if (!_typeToPtr.IsCreated)
				return;
			for (var i = 0; i < _typeToPtr.Length; i++)
			{
				ref var slot = ref _typeToPtr[i];
				if (slot.IsValid)
				{
					MemoryExt.MemFree(slot);
					slot = default;
				}
			}
		}

		public void Dispose()
		{
			if (!_typeToPtr.IsCreated)
				return;
			for (var i = 0; i < _typeToPtr.Length; i++)
			{
				ref var slot = ref _typeToPtr[i];
				if (slot.IsValid)
					MemoryExt.MemFree(slot);
			}
			_typeToPtr.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Has<T>() where T : unmanaged, IWorldLocalUnmanagedService
		{
			if (!_typeToPtr.IsCreated)
				return false;
			return _typeToPtr[TypeIdOf<IWorldLocalUnmanagedService, T>.typeId].IsValid;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetPtr<T>(out SafePtr<T> value) where T : unmanaged, IWorldLocalUnmanagedService
		{
			value = default;
			if (!_typeToPtr.IsCreated)
				return false;
			ref var slot = ref _typeToPtr[TypeIdOf<IWorldLocalUnmanagedService, T>.typeId];
			if (!slot.IsValid)
				return false;
			value = slot;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetPtr<T>() where T : unmanaged, IWorldLocalUnmanagedService
		{
			E.ASSERT(_typeToPtr.IsCreated);
			ref var slot = ref _typeToPtr[TypeIdOf<IWorldLocalUnmanagedService, T>.typeId];
			E.ASSERT(slot.IsValid);
			return slot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T Get<T>() where T : unmanaged, IWorldLocalUnmanagedService
		{
			return ref GetPtr<T>().Value();
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrCreatePtr`, который просто регистрирует).
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>(WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService, IInitializableService
		{
			ref var service = ref GetOrCreatePtr<T>(out var isExist).Value();
			if (!isExist)
				service.Initialize(worldState);
			return ref service;
		}

		/// <summary>
		/// Если сервиса нет, то регистрирует его и инициализирует (В отличие от `GetOrCreatePtr`, который просто регистрирует).
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>(WorldState worldState) where T : unmanaged, IWorldLocalUnmanagedService, IInitializableService
		{
			var servicePtr = GetOrCreatePtr<T>(out var isExist);
			if (!isExist)
				servicePtr.Value().Initialize(worldState);
			return servicePtr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>() where T : unmanaged, IWorldLocalUnmanagedService
		{
			return GetOrCreatePtr<T>(out _);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public SafePtr<T> GetOrCreatePtr<T>(out bool isExist) where T : unmanaged, IWorldLocalUnmanagedService
		{
			EnsureInitialized();
			ref var slot = ref _typeToPtr[TypeIdOf<IWorldLocalUnmanagedService, T>.typeId];
			isExist = slot.IsValid;
			if (!isExist)
				slot = MemoryExt.MemAllocAndClear<T>();
			return slot;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>() where T : unmanaged, IWorldLocalUnmanagedService
		{
			return ref GetOrCreatePtr<T>().Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T GetOrCreate<T>(out bool isExist) where T : unmanaged, IWorldLocalUnmanagedService
		{
			return ref GetOrCreatePtr<T>(out isExist).Value();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<T>() where T : unmanaged, IWorldLocalUnmanagedService
		{
			if (!_typeToPtr.IsCreated)
				return false;
			ref var slot = ref _typeToPtr[TypeIdOf<IWorldLocalUnmanagedService, T>.typeId];
			if (!slot.IsValid)
				return false;
			MemoryExt.MemFree(slot);
			slot = default;
			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Remove<T>(out T value) where T : unmanaged, IWorldLocalUnmanagedService
		{
			value = default;
			if (!_typeToPtr.IsCreated)
				return false;
			ref var slot = ref _typeToPtr[TypeIdOf<IWorldLocalUnmanagedService, T>.typeId];
			if (!slot.IsValid)
				return false;
			value = slot.Value<T>();
			MemoryExt.MemFree(slot);
			slot = default;
			return true;
		}
	}
}
