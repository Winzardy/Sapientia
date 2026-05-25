using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.TypeIndexer;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Generic-хранилище payload'ов в heap-памяти (off-allocator) проиндексированное по
	/// <see cref="TypeIdOf{TBase, T}"/>. Не попадает в снапшот мира.
	/// Не знает о конкретном <typeparamref name="TBase"/>-маркере.
	/// Для cases где payload требует ручного free (например <see cref="Sapientia.Data.SafePtr"/> из MemAlloc) — caller отвечает за освобождение перед Set.
	/// </summary>
	public struct UnsafeIndexedRegistry<TBase, TPayload> : IDisposable
		where TBase : IIndexedType
		where TPayload : unmanaged
	{
		private UnsafeArray<TPayload> _payloads;

		/// <summary>
		/// Factory: автосайз по <see cref="TypeId{TBase}.Count"/> — количество всех типов реализующих <typeparamref name="TBase"/>.
		/// Размер фиксированный, новые типы после генератора TypeIndexer не появятся (full domain reload required).
		/// Static method а не ctor — C# 9.0 не поддерживает parameterless struct constructors (Unity 6000 default).
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static UnsafeIndexedRegistry<TBase, TPayload> Create()
		{
			return new UnsafeIndexedRegistry<TBase, TPayload>
			{
				_payloads = new UnsafeArray<TPayload>(TypeId<TBase>.Count),
			};
		}

		public readonly bool IsCreated
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _payloads.IsCreated;
		}

		public int Length
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _payloads.Length;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TPayload Get<T>() where T : unmanaged, TBase
		{
			return ref _payloads[(int)TypeIdOf<TBase, T>.typeId];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TPayload Get(TypeId<TBase> contextTypeId)
		{
			return ref _payloads[(int)contextTypeId];
		}

		/// <summary>
		/// Прямой доступ по сырому индексу — для iteration / cleanup путей.
		/// Для обычного доступа используй generic <see cref="Get{T}"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TPayload GetByIndex(int index)
		{
			return ref _payloads[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set<T>(in TPayload payload) where T : unmanaged, TBase
		{
			if (!_payloads.IsCreated)
				throw new System.InvalidOperationException("UnsafeIndexedRegistry не инициализирован — должен быть создан через UnsafeIndexedRegistry<,>.Create().");
			_payloads[(int)TypeIdOf<TBase, T>.typeId] = payload;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(TypeId<TBase> contextTypeId, in TPayload payload)
		{
			if (!_payloads.IsCreated)
				throw new System.InvalidOperationException("UnsafeIndexedRegistry не инициализирован — должен быть создан через UnsafeIndexedRegistry<,>.Create().");
			_payloads[(int)contextTypeId] = payload;
		}

		/// <summary>
		/// Сбрасывает все слоты в <c>default</c>. Caller отвечает за освобождение payload'ов до Clear.
		/// </summary>
		public void Clear()
		{
			if (!_payloads.IsCreated)
				return;
			for (var i = 0; i < _payloads.Length; i++)
				_payloads[i] = default;
		}

		public void Dispose()
		{
			if (_payloads.IsCreated)
				_payloads.Dispose();
		}
	}
}
