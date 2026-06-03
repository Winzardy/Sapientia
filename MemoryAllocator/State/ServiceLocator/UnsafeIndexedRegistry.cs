using System;
using System.Runtime.CompilerServices;
using Sapientia.Collections;
using Sapientia.Memory;
using Sapientia.TypeIndexer;
using Submodules.Sapientia.Memory;

namespace Sapientia.MemoryAllocator
{
	/// <summary>
	/// Generic-хранилище payload'ов в heap-памяти (off-allocator) проиндексированное по
	/// <see cref="TypeIdOf{TBase, T}"/>. Не знает о конкретном <typeparamref name="TBase"/>-маркере —
	/// переиспользуется для in-state регистров (ServiceRegistry, ComponentsManager) и heap-only
	/// (noStateServiceRegistry). Сериализация — ручная через <see cref="Serialize"/> / <see cref="Deserialize"/>,
	/// caller решает попадает регистр в снапшот или нет.
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
			return ref Get(TypeIdOf<TBase, T>.typeId);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TPayload Get(TypeId<TBase> typeId)
		{
			E.ASSERT(_payloads.IsCreated);
			return ref _payloads[(int)typeId];
		}

		/// <summary>
		/// Прямой доступ по сырому индексу — для iteration / cleanup путей.
		/// Для обычного доступа используй generic <see cref="Get{T}"/>.
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref TPayload GetByIndex(int index)
		{
			E.ASSERT(_payloads.IsCreated);
			return ref _payloads[index];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set<T>(in TPayload payload) where T : unmanaged, TBase
		{
			Set(TypeIdOf<TBase, T>.typeId, payload);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Set(TypeId<TBase> typeId, in TPayload payload)
		{
			E.ASSERT(_payloads.IsCreated, "UnsafeIndexedRegistry не инициализирован — должен быть создан через UnsafeIndexedRegistry<,>.Create().");
			_payloads[(int)typeId] = payload;
		}

		/// <summary>
		/// Сбрасывает все слоты в <c>default</c>. Caller отвечает за освобождение payload'ов до Clear.
		/// </summary>
		public void Clear()
		{
			if (!_payloads.IsCreated)
				return;
			_payloads.Clear();
		}

		public void Dispose()
		{
			if (_payloads.IsCreated)
				_payloads.Dispose();
		}

		/// <summary>
		/// Записывает в снапшот length + blittable массив payload'ов. Caller решает попадает регистр в снапшот.
		/// </summary>
		public void Serialize(ref StreamBufferWriter stream)
		{
			stream.Write(_payloads.Length);
			if (_payloads.Length > 0)
				stream.Write(_payloads.ptr, _payloads.Length);
		}

		/// <summary>
		/// Восстанавливает регистр из снапшота. Длина массива берётся из стрима — она может отличаться от
		/// <see cref="TypeId{TBase}.Count"/> текущего билда (если генератор перечитал типы между Save и Load),
		/// поэтому payload-ы valid только в рамках одного билда.
		/// </summary>
		public static UnsafeIndexedRegistry<TBase, TPayload> Deserialize(ref StreamBufferReader stream)
		{
			var length = stream.Read<int>();
			var result = new UnsafeIndexedRegistry<TBase, TPayload>
			{
				_payloads = new UnsafeArray<TPayload>(length, ClearOptions.UninitializedMemory),
			};
			if (length > 0)
			{
				var ptr = result._payloads.ptr;
				stream.Read(ptr, length);
			}
			return result;
		}
	}
}
